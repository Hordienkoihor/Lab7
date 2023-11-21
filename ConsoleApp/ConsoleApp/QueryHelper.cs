
using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleApp.Model;
using ConsoleApp.Model.Enum;
using ConsoleApp.OutputTypes;

namespace ConsoleApp
{
    public class QueryHelper : IQueryHelper
    {
       

        public QueryHelper(List<Delivery> deliveries)
        {
            
        }

        public IEnumerable<Delivery> Paid(IEnumerable<Delivery> deliveries) =>
            deliveries.Where(delivery => !string.IsNullOrEmpty(delivery.PaymentId));

        public IEnumerable<Delivery> NotFinished(IEnumerable<Delivery> deliveries) =>
            deliveries.Where(delivery => delivery.Status != DeliveryStatus.Cancelled && delivery.Status != DeliveryStatus.Done);

        public IEnumerable<DeliveryShortInfo> DeliveryInfosByClient(IEnumerable<Delivery> deliveries, string clientId) =>
            deliveries
                .Where(delivery => delivery.ClientId == clientId)
                .Select(delivery => new DeliveryShortInfo
                {
                    Id = delivery.Id,
                    StartCity = delivery.Direction.Origin.City,
                    EndCity = delivery.Direction.Destination.City,
                    ClientId = delivery.ClientId,
                    Type = delivery.Type,
                    LoadingPeriod = delivery.LoadingPeriod,
                    ArrivalPeriod = delivery.ArrivalPeriod,
                    Status = delivery.Status,
                    CargoType = delivery.CargoType
                });

        public IEnumerable<Delivery> DeliveriesByCityAndType(IEnumerable<Delivery> deliveries, string cityName, DeliveryType type) =>
            deliveries
                .Where(delivery => delivery.Direction.Origin.City == cityName && delivery.Type == type)
                .Take(10);

        public IEnumerable<Delivery> OrderByStatusThenByStartLoading(IEnumerable<Delivery> deliveries) =>
            deliveries
                .OrderBy(delivery => delivery.Status)
                .ThenBy(delivery => delivery.LoadingPeriod.Start);

        public int CountUniqCargoTypes(IEnumerable<Delivery> deliveries) =>
            deliveries.Select(delivery => delivery.CargoType).Distinct().Count();

        public Dictionary<DeliveryStatus, int> CountsByDeliveryStatus(IEnumerable<Delivery> deliveries) =>
            deliveries
                .GroupBy(delivery => delivery.Status)
                .ToDictionary(group => group.Key, group => group.Count());

        public IEnumerable<AverageGapsInfo> AverageTravelTimePerDirection(IEnumerable<Delivery> deliveries) =>
            deliveries
                .GroupBy(delivery => new { OriginCity = delivery.Direction.Origin.City, DestinationCity = delivery.Direction.Destination.City })
                .Select(group => new AverageGapsInfo
                {
                    StartCity = group.Key.OriginCity,
                    EndCity = group.Key.DestinationCity,
                    AverageGap = TimeSpan.FromTicks((long)group.Average(delivery =>
                            (delivery.ArrivalPeriod.Start - delivery.LoadingPeriod.End).GetValueOrDefault().Ticks))
                        .TotalMinutes
                });


        public IEnumerable<TElement> Paging<TElement, TOrderingKey>(IEnumerable<TElement> elements,
            Func<TElement, TOrderingKey> ordering,
            Func<TElement, bool>? filter = null,
            int countOnPage = 100,
            int pageNumber = 1) =>
            elements
                .Where(filter ?? (x => true))
                .OrderBy(ordering)
                .Skip((pageNumber - 1) * countOnPage)
                .Take(countOnPage);
    }
}
