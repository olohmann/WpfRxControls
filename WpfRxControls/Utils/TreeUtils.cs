// ==================================================================================
// This Sample Code is provided for the purpose of illustration only and is not 
// intended to be used in a production environment.  THIS SAMPLE CODE AND ANY RELATED 
// INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY 
// AND/OR FITNESS FOR A PARTICULAR PURPOSE.  We grant You a nonexclusive, 
// royalty-free right to use and modify the Sample Code and to reproduce and 
// distribute the object code form of the Sample Code, provided that You agree: 
// (i) to not use Our name, logo, or trademarks to market Your software product in 
// which the Sample Code is embedded; (ii) to include a valid copyright notice on 
// Your software product in which the Sample Code is embedded; and (iii) to 
// indemnify, hold harmless, and defend Us and Our suppliers from and against any 
// claims or lawsuits, including attorneys’ fees, that arise or result from the use 
// or distribution of the Sample Code.
// ==================================================================================

namespace WpfRxControls.Utils
{
    using System.Windows;
    using System.Windows.Media;

    internal static class TreeUtils
    {
        public static T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
            {
                return null;
            }

            T foundChild = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                var childType = child as T;
                if (childType == null)
                {
                    foundChild = FindChild<T>(child);
                    if (foundChild != null)
                    {
                        break;
                    }
                }
                else
                {
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        public static T FindParent<T>(DependencyObject startingObject) where T : DependencyObject
        {
            DependencyObject parent = GetParent(startingObject);

            while (parent != null)
            {
                var foundElement = parent as T;

                if (foundElement != null)
                {
                    return foundElement;
                }

                parent = GetParent(parent);
            }

            return null;
        }

        private static DependencyObject GetParent(DependencyObject element)
        {
            var visual = element as Visual;
            var parent = (visual == null) ? null : VisualTreeHelper.GetParent(visual);

            if (parent == null)
            {
                // Check for a logical parent when no visual was found.
                var frameworkElement = element as FrameworkElement;

                if (frameworkElement != null)
                {
                    parent = frameworkElement.Parent ?? frameworkElement.TemplatedParent;
                }
                else
                {
                    var frameworkContentElement = element as FrameworkContentElement;

                    if (frameworkContentElement != null)
                    {
                        parent = frameworkContentElement.Parent ?? frameworkContentElement.TemplatedParent;
                    }
                }
            }

            return parent;
        }
    }
}