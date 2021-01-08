namespace IctBaden.Presentation.TreeView
{
    public class TreeViewItemEx : ActiveViewModel
    {
        // Add this ItemContainerStyle to your TreeView
        //
        //  <TreeView.ItemContainerStyle>
        //    <Style TargetType="TreeViewItem">
        //        <Setter Property="IsSelected" Value="{Binding IsSelected, Mode=TwoWay}" />
        //        <Setter Property="IsExpanded" Value="{Binding IsExpanded, Mode=TwoWay}" />
        //    </Style>
        //  </TreeView.ItemContainerStyle>

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    NotifyPropertyChanged(nameof(IsSelected));
                }
            }
        }

        private bool isExpanded;
        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if (value != isExpanded)
                {
                    isExpanded = value;
                    NotifyPropertyChanged(nameof(IsExpanded));
                }
            }
        }

    }
}