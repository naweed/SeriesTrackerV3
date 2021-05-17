using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace XGENO.UWP.Controls.FlipView
{
    public static class DependencyObjectExtend
    {
        public static IEnumerable<DependencyObject> GetDescendant(this DependencyObject element)
        {
            var list = new List<DependencyObject>();

            var count = VisualTreeHelper.GetChildrenCount(element);
            
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                list.Add(child);
                list.AddRange(child.GetDescendant());
            }

            return list;
        }

        public static T GetFirstDescendantOfType<T>(this DependencyObject start) where T : DependencyObject
        {
            return start.GetDescendantsOfType<T>().FirstOrDefault();
        }

        public static IEnumerable<T> GetDescendantsOfType<T>(this DependencyObject start) where T : DependencyObject
        {
            return start.GetDescendants().OfType<T>();
        }

        public static IEnumerable<DependencyObject> GetDescendants(this DependencyObject start)
        {
            if (start == null)
                yield break;

            var queue = new Queue<DependencyObject>();
            queue.Enqueue(start);

            yield return start;

            while (queue.Count > 0)
            {
                var parent = queue.Dequeue();
                var count2 = VisualTreeHelper.GetChildrenCount(parent);

                for (int i = 0; i < count2; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    yield return child;
                    queue.Enqueue(child);
                }
            }
        }
    }
}
