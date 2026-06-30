using System;
using System.Collections.Generic;

namespace WaterEquipmentRental
{
    public interface IElectricEquipment
    {
        void Charge();
    }

    public abstract class WaterEquipment
    {
        private string _id;
        private string _brand;
        private string _model;
        private decimal _rentalRate;
        private bool _isAvailable;

        public string Id 
        { 
            get => _id; 
            private set => _id = !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException("ID cannot be empty"); 
        }
        
        public string Brand { get => _brand; set => _brand = value; }
        public string Model { get => _model; set => _model = value; }
        
        public decimal RentalRate 
        { 
            get => _rentalRate; 
            set => _rentalRate = value > 0 ? value : throw new ArgumentException("Rate must be greater than zero"); 
        }
        
        public bool IsAvailable { get => _isAvailable; set => _isAvailable = value; }

        protected WaterEquipment(string id, string brand, string model, decimal rentalRate)
        {
            Id = id;
            Brand = brand;
            Model = model;
            RentalRate = rentalRate;
            IsAvailable = true;
        }

        public void Rent()
        {
            if (!IsAvailable) throw new InvalidOperationException("Equipment is already rented");
            IsAvailable = false;
        }

        public void ReturnEquipment()
        {
            IsAvailable = true;
        }

        public abstract string GetEquipmentType();
    }

    public class JetSki : WaterEquipment
    {
        public int EnginePower { get; set; }

        public JetSki(string id, string brand, string model, decimal rentalRate, int enginePower)
            : base(id, brand, model, rentalRate)
        {
            EnginePower = enginePower;
        }

        public override string GetEquipmentType()
        {
            return $"Skuter wodny (Moc: {EnginePower} KM)";
        }
    }

    public class Kayak : WaterEquipment
    {
        public int SeatsCount { get; set; }

        public Kayak(string id, string brand, string model, decimal rentalRate, int seatsCount)
            : base(id, brand, model, rentalRate)
        {
            SeatsCount = seatsCount;
        }

        public override string GetEquipmentType()
        {
            return $"Kajak ({SeatsCount}-osobowy)";
        }
    }

    public class ElectricBoat : WaterEquipment, IElectricEquipment
    {
        public int BatteryCapacity { get; set; }

        public ElectricBoat(string id, string brand, string model, decimal rentalRate, int batteryCapacity)
            : base(id, brand, model, rentalRate)
        {
            BatteryCapacity = batteryCapacity;
        }

        public void Charge()
        {
            Console.WriteLine($"Ładowanie łodzi elektrycznej {Brand} {Model}. Pojemność akumulatora: {BatteryCapacity} kWh.");
        }

        public override string GetEquipmentType()
        {
            return $"Łódź elektryczna (Bateria: {BatteryCapacity} kWh)";
        }
    }

    public class User
    {
        public string AlbumNumber { get; private set; }
        public string FullName { get; set; }
        public List<string> RentalHistory { get; private set; }

        public User(string albumNumber, string fullName)
        {
            AlbumNumber = albumNumber;
            FullName = fullName;
            RentalHistory = new List<string>();
        }

        public void AddToHistory(string entry)
        {
            RentalHistory.Add(entry);
        }
    }

    public class Rental
    {
        public string RentalId { get; private set; }
        public User Customer { get; private set; }
        public WaterEquipment Equipment { get; private set; }
        public DateTime RentalDate { get; private set; }

        public Rental(string rentalId, User customer, WaterEquipment equipment)
        {
            RentalId = rentalId;
            Customer = customer;
            Equipment = equipment;
            RentalDate = DateTime.Now;
        }
    }

    public class RentalManager
    {
        private List<WaterEquipment> _inventory = new List<WaterEquipment>();
        private List<Rental> _activeRentals = new List<Rental>();

        public void AddEquipment(WaterEquipment equipment)
        {
            _inventory.Add(equipment);
        }

        public void ProcessRental(string rentalId, User user, string equipmentId)
        {
            WaterEquipment equipment = _inventory.Find(e => e.Id == equipmentId);
            if (equipment == null) return;

            if (equipment.IsAvailable)
            {
                equipment.Rent();
                Rental rental = new Rental(rentalId, user, equipment);
                _activeRentals.Add(rental);
                user.AddToHistory($"Wypożyczono: {equipment.GetEquipmentType()} ({equipment.Brand} {equipment.Model}) dnia {rental.RentalDate}");
            }
        }

        public void ProcessReturn(string equipmentId)
        {
            Rental rental = _activeRentals.Find(r => r.Equipment.Id == equipmentId);
            if (rental != null)
            {
                rental.Equipment.ReturnEquipment();
                _activeRentals.Remove(rental);
            }
        }

        public void DisplayAvailableEquipment()
        {
            foreach (var item in _inventory)
            {
                if (item.IsAvailable)
                {
                    Console.WriteLine($"[{item.Id}] {item.GetEquipmentType()} - {item.Brand} {item.Model} ({item.RentalRate} PLN/h)");
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            RentalManager manager = new RentalManager();

            JetSki ski = new JetSki("SW-01", "Yamaha", "FX Cruiser", 250, 180);
            Kayak kayak = new Kayak("SW-02", "Perception", "Prodigy II", 40, 2);
            ElectricBoat boat = new ElectricBoat("SW-03", "Rand", "Mana 23", 400, 30);

            manager.AddEquipment(ski);
            manager.AddEquipment(kayak);
            manager.AddEquipment(boat);

            User student = new User("123456", "Kamil Bugaj");

            Console.WriteLine("--- Dostępny sprzęt wodny ---");
            manager.DisplayAvailableEquipment();

            Console.WriteLine("\n--- Proces wypożyczenia ---");
            manager.ProcessRental("R-2026-01", student, "SW-01");
            manager.ProcessRental("R-2026-02", student, "SW-03");

            Console.WriteLine("\n--- Dostępny sprzęt po wypożyczeniu ---");
            manager.DisplayAvailableEquipment();

            Console.WriteLine("\n--- Polimorficzne wywołanie metod ---");
            List<WaterEquipment> items = new List<WaterEquipment> { ski, kayak, boat };
            foreach (var item in items)
            {
                Console.WriteLine($"Typ obiektu w liście: {item.GetEquipmentType()}");
            }

            Console.WriteLine("\n--- Test interfejsu (Abstrakcja) ---");
            boat.Charge();

            Console.WriteLine("\n--- Historia wypożyczeń użytkownika ---");
            foreach (var record in student.RentalHistory)
            {
                Console.WriteLine(record);
            }

            manager.ProcessReturn("SW-01");
            Console.WriteLine("\n--- Dostępny sprzęt po zwrocie skuteru ---");
            manager.DisplayAvailableEquipment();
        }
    }
}
