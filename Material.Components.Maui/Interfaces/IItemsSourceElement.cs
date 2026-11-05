using System.Collections;
using System.Collections.Specialized;

namespace Material.Components.Maui.Interfaces;

public interface IItemsSourceElement
{
    IList ItemsSource { get; set; }
    void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e);

    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
     nameof(ItemsSource),
     typeof(IList),
     typeof(IItemsSourceElement),
     default,
     propertyChanged: (bo, ov, nv) =>
     {
         var element = (IItemsSourceElement)bo;

         if (ov is INotifyCollectionChanged oldNcc)
         {
             oldNcc.CollectionChanged -= element.OnItemsSourceCollectionChanged;
         }

         if (nv is IList initialList)
         {
             element.OnItemsSourceCollectionChanged(
                 element,
                 new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)
             );

             if (initialList.Count > 0)
             {
                 element.OnItemsSourceCollectionChanged(
                     element,
                     new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, initialList, 0)
                 );
             }
         }

         if (nv is INotifyCollectionChanged newNcc)
         {
             newNcc.CollectionChanged += element.OnItemsSourceCollectionChanged;
         }
     }
 );
}
