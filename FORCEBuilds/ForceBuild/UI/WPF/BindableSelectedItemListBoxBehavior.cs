using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Interactivity;
using Xunit;

namespace FORCEBuild.UI.WPF
{
    public class BindableSelectedItemListBoxBehavior:Behavior<ListBox>
    {
       /// <summary>
       /// 只读
       /// </summary>
        public IList SelectedItems   
        {
            get { return (IList) GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(nameof(SelectedItems), typeof(IList),
                typeof(BindableSelectedItemListBoxBehavior),
                new FrameworkPropertyMetadata(new ArrayList()) {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

        //public object SelectedItem
        //{
        //    get { return (object)GetValue(SelectedItemProperty); }
        //    set { SetValue(SelectedItemProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty SelectedItemProperty =
        //    DependencyProperty.Register("SelectedItem", typeof(object), 
        //        typeof(BindableSelectedItemListBoxBehavior), 
        //        new FrameworkPropertyMetadata(null,OnSelectedItemChanged) {
        //            BindsTwoWayByDefault = true,
        //            DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        //        });

        //private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        //{
        //    var item = e.NewValue as ListBoxItem;
        //    item?.SetValue(ListBoxItem.IsSelectedProperty, true);

        //}


        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }
        
        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (this.AssociatedObject!=null) {
                this.AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
            }
        }



        private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.SelectedItems = AssociatedObject.SelectedItems;
         //   this.SelectedItem = AssociatedObject.SelectedItem;
            //if (e.AddedItems.Count>0) {
            //    this.SelectedItems.Add(e.AddedItems);
            //}
            //if (e.RemovedItems.Count>0) {
            //    foreach (var item in e.RemovedItems) {
            //        SelectedItems.Remove(item);
            //    }
            //}
        }
    }
}