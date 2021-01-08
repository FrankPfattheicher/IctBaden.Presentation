using IctBaden.Presentation.Framework;

namespace IctBaden.Presentation.TreeView
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;

    public class TreeViewEx : TreeView
    {
        public TreeViewEx()
        {
            SelectedItemChanged += TreeViewExSelectedItemChanged;
        }

        void TreeViewExSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var newValue = e.NewValue;
            if (!string.IsNullOrEmpty(SelectedItemType))
            {
                var type =
                    AppDomain.CurrentDomain.GetAssemblies()
                        .Where(a => a.GetType(SelectedItemType) != null)
                        .Select(a => a.GetType(SelectedItemType))
                        .FirstOrDefault();
                if (type != null)
                {
                    newValue = UniversalConverter.ConvertToType(newValue, type);
                }
            }
            SelectedItem = newValue;
        }

        #region SelectedItem

        /// <summary>
        /// Gets or Sets the SelectedItem possible Value of the TreeViewItem object.
        /// </summary>
        public new object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        /// Gets or Sets the SelectedItem possible Value of the TreeViewItem object.
        /// </summary>
        public string SelectedItemType
        {
            get { return (string)GetValue(SelectedItemTypeProperty); }
            set { SetValue(SelectedItemTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public new static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(TreeViewEx),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedItemPropertyChanged));
        public static readonly DependencyProperty SelectedItemTypeProperty =
            DependencyProperty.Register("SelectedItemType", typeof(string), typeof(TreeViewEx),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.NotDataBindable));

        static void SelectedItemPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var targetObject = dependencyObject as TreeViewEx;
            if (targetObject == null)
            {
                return;
            }
            var tvi = targetObject.FindItemNode(targetObject.SelectedItem);
            if (tvi != null)
                tvi.IsSelected = true;
        }
        #endregion SelectedItem

        public TreeViewItem FindItemNode(object item)
        {
            TreeViewItem node = null;
            foreach (object data in Items)
            {
                node = ItemContainerGenerator.ContainerFromItem(data) as TreeViewItem;
                if (node != null)
                {
                    if (data == item)
                        break;
                    node = FindItemNodeInChildren(node, item, false);
                    if (node != null)
                        break;
                }
            }
            return node;
        }

        protected TreeViewItem FindItemNodeInChildren(TreeViewItem parent, object item, bool expandToFind)
        {
            if (parent == null)
                return null;

            TreeViewItem node = null;
            var isExpanded = parent.IsExpanded;
            if (!isExpanded && expandToFind) //Can't find child container unless the parent node is Expanded once
            {
                parent.IsExpanded = true;
                parent.UpdateLayout();
            }
            foreach (object data in parent.Items)
            {
                node = parent.ItemContainerGenerator.ContainerFromItem(data) as TreeViewItem;
                if (data == item && node != null)
                    break;
                node = FindItemNodeInChildren(node, item, expandToFind);
                if (node != null)
                    break;
            }
            if (node == null && parent.IsExpanded != isExpanded)
                parent.IsExpanded = isExpanded;
            if (node != null)
                parent.IsExpanded = true;
            return node;
        }
    }
}
