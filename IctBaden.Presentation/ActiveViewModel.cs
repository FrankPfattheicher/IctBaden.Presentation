// ActiveViewModel.cs
//
// Author:
//  Frank Pfattheicher <fpf@ict-baden.de>
//
// Copyright (C)2011-2020 ICT Baden GmbH
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
// ReSharper disable UnusedMember.Global

namespace IctBaden.Presentation
{
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public class ActiveViewModel : DynamicObject, ICustomTypeDescriptor, ICustomTypeProvider, INotifyPropertyChanged, IDisposable
    {
        #region helper classes

        private class GetMemberBinderEx : GetMemberBinder
        {
            public GetMemberBinderEx(string name) : base(name, false) { }
            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
            { return null; }
        }

        private class SetMemberBinderEx : SetMemberBinder
        {
            public SetMemberBinderEx(string name) : base(name, false) { }
            public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
            { return null; }
        }

        private class PropertyDescriptorEx : PropertyDescriptor
        {
            private readonly string _propertyName;
            private readonly PropertyDescriptor _originalDescriptor;

            internal PropertyDescriptorEx(string name, PropertyInfo info)
                : base(name, null)
            {
                _propertyName = name;
                _originalDescriptor = FindOrigPropertyDescriptor(info);
            }

            public override AttributeCollection Attributes => _originalDescriptor?.Attributes ?? base.Attributes;

            public override object GetValue(object component)
            {
                if (component is DynamicObject dynComponent)
                {
                    if (dynComponent.TryGetMember(new GetMemberBinderEx(_propertyName), out var result))
                    {
                        var command = result as DelegateCommand;
                        command?.SignalCanExecuteChanged();
                        return result;
                    }
                }
                return _originalDescriptor?.GetValue(component);
            }

            public override void SetValue(object component, object value)
            {
                if (component is DynamicObject dynComponent)
                {
                    if (dynComponent.TrySetMember(new SetMemberBinderEx(_propertyName), value))
                        return;
                }
                _originalDescriptor?.SetValue(component, value);
            }

            public override bool IsReadOnly => _originalDescriptor != null && _originalDescriptor.IsReadOnly;

            public override Type PropertyType => (_originalDescriptor == null) ? typeof(object) : _originalDescriptor.PropertyType;

            public override bool CanResetValue(object component)
            {
                return _originalDescriptor != null && _originalDescriptor.CanResetValue(component);
            }

            public override Type ComponentType => (_originalDescriptor == null) ? typeof(object) : _originalDescriptor.ComponentType;

            public override void ResetValue(object component)
            {
                _originalDescriptor?.ResetValue(component);
            }

            public override bool ShouldSerializeValue(object component)
            {
                return _originalDescriptor != null && _originalDescriptor.ShouldSerializeValue(component);
            }

            private static PropertyDescriptor FindOrigPropertyDescriptor(PropertyInfo propertyInfo)
            {
                return (propertyInfo == null) ? null : TypeDescriptor.GetProperties(propertyInfo.DeclaringType).Cast<PropertyDescriptor>().FirstOrDefault(propertyDescriptor => propertyDescriptor.Name.Equals(propertyInfo.Name));
            }
        }

        private class PropertyInfoEx
        {
            public PropertyInfo Info { get; }
            public object Obj { get; }

            public PropertyInfoEx(PropertyInfo pi, object obj)
            {
                Info = pi;
                Obj = obj;
            }
        }

        private class DynamicModel
        {
            public readonly string Prefix;
            public readonly string TypeName;
            public object Model;

            public DynamicModel(string prefix, object model)
            {
                Prefix = prefix;
                Model = model;
                TypeName = model.GetType().Name;
            }
        }

        #endregion

        #region properties

        private static bool? _isInDesignMode;
        public static bool IsInDesignMode
        {
            get
            {
                if (_isInDesignMode.HasValue) return _isInDesignMode.Value;

                _isInDesignMode = (bool)DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty, typeof(FrameworkElement)).Metadata.DefaultValue;
                if (!_isInDesignMode.Value && Process.GetCurrentProcess().ProcessName.StartsWith("devenv", StringComparison.Ordinal))
                    _isInDesignMode = true;
                return _isInDesignMode.Value;
            }
        }

        protected readonly Dispatcher UiDispatcher;
        protected readonly Window View;

        private readonly Dictionary<string, List<string>> _dependencies = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, object> _dictionary = new Dictionary<string, object>();
        private readonly List<DynamicModel> _models = new List<DynamicModel>();
        [Browsable(false)]
        internal int Count => GetProperties().Count;

        [Browsable(false)]
        internal IEnumerable<string> Models => from model in _models select model.GetType().Name;

        internal bool ModelTypeExists(string prefix, object model) { return _models.FirstOrDefault(m => (m.TypeName == model.GetType().Name) && (m.Prefix == prefix)) != null; }

        #endregion

        public ActiveViewModel()
            : this(null)
        {
        }

        internal ActiveViewModel(Window view)
        {
            BindingPriority = DispatcherPriority.DataBind;

            if (!IsInDesignMode && (Application.Current != null) &&
              (Application.Current?.Dispatcher?.CheckAccess() == true) && (Application.Current.MainWindow != null))
            {
                View = view ?? Application.Current.MainWindow;
                UiDispatcher = View.Dispatcher;
                View.Loaded += (sender, args) => OnViewLoaded();
                View.Unloaded += (sender, args) => OnViewUnloaded();
                View.Closing += (sender, args) => OnViewClosing();
                View.Closed += (sender, args) => OnViewClosed();
            }
            else
            {
                UiDispatcher = Dispatcher.CurrentDispatcher;
            }
            BindCommands(this);
        }
        // ReSharper disable once UnusedMember.Global
        public ActiveViewModel(object model)
            : this()
        {
            SetModel(model);
        }

        ~ActiveViewModel() => Dispose();

        public void Dispose()
        {
            _dependencies.Clear();
            _properties?.Clear();
            _dependencies.Clear();
            _dictionary.Clear();
            _models.Clear();
        }


        public override string ToString()
        {
            return (_models.Count > 0) ? _models[0].Model.ToString() : string.Empty;
        }

        // ReSharper disable once UnusedMember.Global
        protected void SetParent(ActiveViewModel parent)
        {
            PropertyChanged += (sender, args) => parent.NotifyPropertyChanged(args.PropertyName);
        }

        public virtual void OnViewLoaded()
        { }
        public virtual void OnViewUnloaded()
        { }
        public virtual void OnViewClosing()
        { }
        public virtual void OnViewClosed()
        { }

        private void BindCommands(object model)
        {
            var myMethods = model.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            foreach (var method in myMethods)
            {
                var actionAttributes = method.GetCustomAttributes(typeof(ActionMethodAttribute), true);
                foreach (ActionMethodAttribute attribute in actionAttributes)
                {
                    if (string.IsNullOrEmpty(attribute.Name))
                    {
                        attribute.Name = method.Name;
                    }

                    var currentMethod = method;
                    Predicate<object> enabled = null;

                    if (!string.IsNullOrEmpty(attribute.EnabledPredicate))
                    {
                        var en = model.GetType().GetMethod(attribute.EnabledPredicate, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                        if ((en != null) && (en.ReturnType == typeof(bool)))
                        {
                            enabled = s => (bool)en.Invoke(this, new object[] { });
                        }
                    }

                    if (!string.IsNullOrEmpty(attribute.Trigger))
                    {
                        var trigger = attribute.Trigger;
                        var exec = model.GetType().GetMethod(attribute.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                        PropertyChanged += (sender, args) => { if (args.PropertyName == trigger) exec?.Invoke(model, new object[] { }); };
                    }

                    try
                    {
                        var execAction = (currentMethod.GetParameters().Length > 0)
                            ? new Action<object>(param => currentMethod.Invoke(model, new[] { param }))
                            : (param => currentMethod.Invoke(model, new object[] { }));

                        var command = attribute.ExecuteAsync
                            ? new DelegateCommand(UiDispatcher, param => Task.Factory.StartNew(() => execAction(param)), enabled)
                            : new DelegateCommand(UiDispatcher, execAction, enabled);

                        this[attribute.Name] = command;
                        var gestureConverter = new KeyGestureConverter();
                        if (!IsInDesignMode && !string.IsNullOrEmpty(attribute.Shortcut) &&
                            gestureConverter.IsValid(attribute.Shortcut))
                        {
                            var gesture = (KeyGesture)gestureConverter.ConvertFromString(attribute.Shortcut);
                            if (gesture != null)
                            {
                                var binding = new KeyBinding(command, gesture);
                                Application.Current.MainWindow.InputBindings.Add(binding);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
        }

        public object this[string name]
        {
            get
            {
                TryGetMember(new GetMemberBinderEx(name), out object result);
                return result;
            }
            set => TrySetMember(new SetMemberBinderEx(name), value);
        }

        public void SetModel(object model)
        {
            SetModel(null, model);
        }
        public void SetModel(string prefix, object model)
        {
            if (ModelTypeExists(prefix, model))
            {
                //throw new ArgumentException(string.Format("A model of type '{0}' is already added", model.GetType().Name));
                UpdateModel(prefix, model);
                return;
            }
            _models.Add(new DynamicModel(prefix, model));
            BindCommands(model);

            _properties = null;
            _propertiesAttribute = null;
            GetProperties();
        }

        public void UpdateModel(object model)
        {
            UpdateModel(null, model);
        }
        public void UpdateModel(string prefix, object model)
        {
            if (!ModelTypeExists(prefix, model))
                throw new ArgumentException($"No model of type '{model.GetType().Name}' is added");

            var index = (from m in _models where (m.TypeName == model.GetType().Name) && (m.Prefix == prefix) select _models.IndexOf(m)).First();
            _models[index].Model = model;
            foreach (var prop in model.GetType().GetProperties())
            {
                if (string.IsNullOrEmpty(prefix))
                {
                    NotifyPropertyChanged(prop.Name);
                }
                else
                {
                    NotifyPropertyChanged(prefix + prop.Name);
                }
            }
        }

        #region DynamicObject

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            var names = new List<string>();
            foreach (var model in _models)
                names.AddRange(from prop in model.GetType().GetProperties() select prop.Name);
            names.AddRange(from elem in _dictionary select elem.Key);
            return names;
        }

        private PropertyInfoEx GetPropertyInfo(string name)
        {
            var pi = GetType().GetProperty(name);
            if (pi != null)
            {
                return new PropertyInfoEx(pi, this);
            }
            foreach (var model in _models)
            {
                if (!string.IsNullOrEmpty(model.Prefix))
                {
                    if (!name.StartsWith(model.Prefix))
                        continue;
                    name = name.Substring(model.Prefix.Length);
                }
                pi = model.Model.GetType().GetProperty(name);
                if (pi == null)
                    continue;

                return new PropertyInfoEx(pi, model.Model);
            }
            return null;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            try
            {
                var pi = GetPropertyInfo(binder.Name);
                if (pi != null)
                {
                    var val = pi.Info.GetValue(pi.Obj, null);
                    result = val;
                    return true;
                }
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
            return _dictionary.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            try
            {
                var pi = GetPropertyInfo(binder.Name);
                if (pi != null)
                {
                    pi.Info.SetValue(pi.Obj, value, null);
                    NotifyPropertyChanged(binder.Name);
                    return true;
                }
                _dictionary[binder.Name] = value;
                NotifyPropertyChanged(binder.Name);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        private PropertyDescriptorCollection _properties;
        public PropertyDescriptorCollection GetProperties()
        {
            if (_properties != null)
                return _properties;

            _properties = new PropertyDescriptorCollection(new PropertyDescriptor[0]);
            foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(this, true))
            {
                var pi = GetType().GetProperty(prop.Name);
                var desc = new PropertyDescriptorEx(prop.Name, pi);
                _properties.Add(desc);
            }
            foreach (var model in _models)
            {
                foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(model.Model, true))
                {
                    var pi = model.Model.GetType().GetProperty(prop.Name);
                    var name = prop.Name;
                    if (!string.IsNullOrEmpty(model.Prefix))
                        name = model.Prefix + name;
                    var desc = new PropertyDescriptorEx(name, pi);
                    _properties.Add(desc);
                }
            }
            foreach (var elem in _dictionary)
            {
                var desc = new PropertyDescriptorEx(elem.Key, null);
                _properties.Add(desc);
            }

            foreach (PropertyDescriptorEx prop in _properties)
            {
                foreach (Attribute attribute in prop.Attributes)
                {
                    if (attribute.GetType() != typeof(DependsOnAttribute))
                        continue;
                    var da = (DependsOnAttribute)attribute;
                    if (!_dependencies.ContainsKey(da.Name))
                        _dependencies[da.Name] = new List<string>();

                    _dependencies[da.Name].Add(prop.Name);
                }
            }

            var myMethods = GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            foreach (var method in myMethods)
            {
                var dependsOnAttributes = method.GetCustomAttributes(typeof(DependsOnAttribute), true);
                foreach (DependsOnAttribute attribute in dependsOnAttributes)
                {
                    if (!_dependencies.ContainsKey(attribute.Name))
                        _dependencies[attribute.Name] = new List<string>();

                    _dependencies[attribute.Name].Add(method.Name);
                }
            }

            return _properties;
        }

        private PropertyDescriptorCollection _propertiesAttribute;
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            if (_propertiesAttribute != null)
                return _propertiesAttribute;

            _propertiesAttribute = new PropertyDescriptorCollection(new PropertyDescriptor[0]);
            foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(this, true))
                _propertiesAttribute.Add(prop);
            foreach (var model in _models)
            {
                foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(model, attributes, true))
                    _propertiesAttribute.Add(prop);
            }
            return _propertiesAttribute;
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #region INotifyPropertyChanged

        [Browsable(false)]
        public DispatcherPriority BindingPriority { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ExecuteHandler(PropertyChangedEventHandler handler, string name)
        {
            var dispatcherObject = handler.Target as DispatcherObject;
            var args = new PropertyChangedEventArgs(name);
            // If the subscriber is a DispatcherObject and different thread
            if (dispatcherObject != null && dispatcherObject.CheckAccess() == false)
            {
                // Invoke handler in the target dispatcher's thread
                dispatcherObject.Dispatcher?.BeginInvoke(handler, BindingPriority, this, args);
            }
            else // Execute handler as is
            {
                handler(this, args);
            }
        }

        protected void NotifyPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                ExecuteHandler(handler, name);
            }

            if (!_dependencies.ContainsKey(name))
                return;

            foreach (var dependencyName in _dependencies[name])
            {
                if (this[dependencyName] is DelegateCommand cmd)
                {
                    cmd.SignalCanExecuteChanged();
                }
                else if (handler != null)
                {
                    ExecuteHandler(handler, dependencyName);
                }
            }
        }

        // ReSharper disable once UnusedMember.Global
        protected void NotifyPropertyChanged<T>(Expression<Func<T>> propertySelector)
        {
            var handler = PropertyChanged;
            if (handler == null)
                return;
            var memberExpression = propertySelector.Body as MemberExpression;
            if (memberExpression == null)
                return;
            ExecuteHandler(handler, memberExpression.Member.Name);
        }

        protected void NotifyPropertiesChanged(string[] names)
        {
            foreach (var name in names)
            {
                NotifyPropertyChanged(name);
            }
        }

        #endregion

        #region ICustomTypeProvider
        public Type GetCustomType()
        {
            return GetType();
        }
        #endregion
    }

}
