using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ScrollTest
{
    /// <summary>
    /// 在 ScrollViewer 中定位到指定的控件
    /// 说明：目前支持的是垂直滚动
    /// </summary>
    public class ScrollToControlAction : TriggerAction<FrameworkElement>
    {
        #region DP

        public static readonly DependencyProperty ScrollViewerProperty =
            DependencyProperty.Register("ScrollViewer", typeof(ScrollViewer), typeof(ScrollToControlAction), new PropertyMetadata(null));

        public static readonly DependencyProperty TargetControlProperty =
            DependencyProperty.Register("TargetControl", typeof(FrameworkElement), typeof(ScrollToControlAction), new PropertyMetadata(null));

        //显示名称 
        // Using a DependencyProperty as the backing store for IsActiveName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsActiveNameProperty =
            DependencyProperty.RegisterAttached("IsActiveName", typeof(string), typeof(ScrollToControlAction), new PropertyMetadata(string.Empty));

        // Using a DependencyProperty as the backing store for ScrollViewerSon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollToControlProperty =
            DependencyProperty.RegisterAttached("ScrollToControl", typeof(bool), typeof(ScrollToControlAction), new PropertyMetadata(ScrollToControlHandle));

        // Using a DependencyProperty as the backing store for TargetName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetControlsProperty =
            DependencyProperty.RegisterAttached("TargetControls", typeof(FrameworkElement), typeof(ScrollToControlAction), new PropertyMetadata(null, TargetControlsHandle));

        #endregion

        #region Methods
        private static void ScrollToControlHandle(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ScrollViewer scroll = (ScrollViewer)d;
            scroll.ScrollChanged += Scroll_ScrollChanged;
        }

        private static void Scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var target = (ScrollViewer)sender;
            var endHeight = target.ScrollableHeight;

            var targetFindControl = TargetControls.Find(x => true);
            var targetFindLastControl = TargetControls.FindLast(x => true);         

            if (e.VerticalOffset == 0)
            {
                OldOffsetValue = e.VerticalOffset;
                CurrentTargetControl = targetFindControl;
                SetIsActiveName(target, targetFindControl.Name);
                return;
            }

            if (e.VerticalOffset == endHeight)
            {
                OldOffsetValue = e.VerticalOffset;
                CurrentTargetControl = targetFindLastControl;
                SetIsActiveName(target, targetFindLastControl.Name);
                return;
            }

            //向下
            if (OldOffsetValue < e.VerticalOffset)
            {
                OldOffsetValue = e.VerticalOffset;
                if (targetFindLastControl.Name == CurrentTargetControl.Name) return;

                int currentIndex = TargetControls.FindIndex(x => x.Name == CurrentTargetControl.Name);
                var currentTarget = TargetControls[currentIndex + 1];

                var point = new Point(0, OldOffsetValue);
                var targetPosition = currentTarget.TransformToVisual(target).Transform(point);
                //更新
                if (point.Y > targetPosition.Y)
                {
                    CurrentTargetControl = currentTarget;
                    SetIsActiveName(target, currentTarget.Name);
                }
            }
            //向上
            else
            {
                OldOffsetValue = e.VerticalOffset;
                if (targetFindControl.Name == CurrentTargetControl.Name) return;

                int currentIndex = TargetControls.FindIndex(x => x.Name == CurrentTargetControl.Name);
                var currentTarget = TargetControls[currentIndex - 1];

                var point = new Point(0, OldOffsetValue);
                var targetPosition = currentTarget.TransformToVisual(target).Transform(point);
                //更新
                if (point.Y < targetPosition.Y)
                {
                    CurrentTargetControl = currentTarget;
                    SetIsActiveName(target, currentTarget.Name);
                }
            }
        }

        private static void TargetControlsHandle(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TargetControls.Add(e.NewValue as FrameworkElement);
        }

        protected override void Invoke(object parameter)
        {
            if (TargetControl == null || ScrollViewer == null)
            {
                throw new ArgumentNullException($"{ScrollViewer} or {TargetControl} cannot be null");
            }

            if (TargetControl.Name == CurrentTargetControl.Name) return;

            // 检查指定的控件是否在指定的 ScrollViewer 中
            // TODO: 这里只是指定离它最近的 ScrollViewer，并没有继续向上找
            var container = TargetControl.FindParent<ScrollViewer>();
            if (container == null || container != ScrollViewer)
            {
                throw new Exception("The TargetControl is not in the target ScrollViewer");
            }

            // 获取要定位之前 ScrollViewer 目前的滚动位置
            var currentScrollPosition = ScrollViewer.VerticalOffset;
            var point = new Point(0, currentScrollPosition);

            // 计算出目标位置并滚动
            var targetPosition = TargetControl.TransformToVisual(ScrollViewer).Transform(point);
            ScrollViewer.ScrollToVerticalOffset(targetPosition.Y);

            CurrentTargetControl = TargetControl;
            SetIsActiveName(ScrollViewer, CurrentTargetControl.Name);
        }
        #endregion

        #region Private
        private static FrameworkElement CurrentTargetControl { get; set; }

        private static List<FrameworkElement> TargetControls = new List<FrameworkElement>();

        private static double OldOffsetValue;
        #endregion

        #region Public
        /// <summary>
        /// 目标 ScrollViewer
        /// </summary>
        public ScrollViewer ScrollViewer
        {
            get { return (ScrollViewer)GetValue(ScrollViewerProperty); }
            set { SetValue(ScrollViewerProperty, value); }
        }

        /// <summary>
        /// 要定位的到的控件
        /// </summary>
        public FrameworkElement TargetControl
        {
            get { return (FrameworkElement)GetValue(TargetControlProperty); }
            set { SetValue(TargetControlProperty, value); }
        }

        public static string GetIsActiveName(ScrollViewer obj)
        {
            return (string)obj.GetValue(IsActiveNameProperty);
        }

        public static void SetIsActiveName(ScrollViewer obj, string value)
        {
            obj.SetValue(IsActiveNameProperty, value);
        }

        public static bool GetScrollToControl(ScrollViewer obj)
        {
            return (bool)obj.GetValue(ScrollToControlProperty);
        }
        public static void SetScrollToControl(ScrollViewer obj, bool value)
        {
            obj.SetValue(ScrollToControlProperty, value);
        }

        public static FrameworkElement GetTargetControls(DependencyObject obj)
        {
            return (FrameworkElement)obj.GetValue(TargetControlsProperty);
        }

        public static void SetTargetControls(DependencyObject obj, string value)
        {
            obj.SetValue(TargetControlsProperty, value);
        }
        #endregion
    }
}