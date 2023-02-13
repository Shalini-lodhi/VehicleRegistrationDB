using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;

namespace VehicleRegistrationAuthority
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var repository = new VehicleRepository(client, "VehicleDB", "Vehicles");
            var service = new VehicleService(repository.Vehicles);

            var vehicle1 = new Vehicle
            {
                VIN = "1A2B3C4D5E6F7G8H9I",
                Make = "Mahindra",
                Model = "Bolero",
                Year = 2015,
                RegistrationDate = DateTime.Now,
                Owners = new List<Owner>()
            };
            var vehicle2 = new Vehicle
            {
                VIN = "123456789ABCDEFGHI",
                Make = "Mahindra",
                Model = "XUV700",
                Year = 2023,
                RegistrationDate = DateTime.Now,
                Owners = new List<Owner>()
            };


            vehicle1 = service.RegisterVehicle(vehicle1);
            vehicle2 = service.RegisterVehicle(vehicle2);
            Console.WriteLine("Vehicle Registered Successfully");

            vehicle1.Make = "Suzuki";
            service.UpdateVehicle(vehicle1.Id, vehicle1);
            Console.WriteLine("Vehicle Details Updated Successfully");

            var newOwner1 = new Owner
            {
                Name = "John Doe",
                Address = "123 Main St",
                PhoneNumber = "000-111-2222",
                Email = "john.doe@email.com",
                TransferDate = DateTime.Now
            };
            var newOwner2 = new Owner
            {
                Name = "Jack man",
                Address = "094 Narrow lane",
                PhoneNumber = "098-087-8782",
                Email = "jack.man.one@email.com",
                TransferDate = DateTime.Now
            };
            service.TransferOwnership(vehicle1.Id, newOwner1);
            Console.WriteLine("Ownership Transferred Successfully");

            //transfering ownership
            service.TransferOwnership(vehicle1.Id, newOwner2);
            service.TransferOwnership(vehicle2.Id, newOwner1);
            Console.WriteLine("Successfully Transferred Ownership");

        }
    }
    /*Vehicle class represents a single vehicle and its properties, including its VIN, make, model, year, registration date, and a list of owners*/
    public class Vehicle
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("vin")]
        public string VIN { get; set; }

        [BsonElement("make")]
        public string Make { get; set; }

        [BsonElement("model")]
        public string Model { get; set; }

        [BsonElement("year")]
        public int Year { get; set; }

        [BsonElement("registration_date")]
        public DateTime RegistrationDate { get; set; }

        [BsonElement("owners")]
        public List<Owner> Owners { get; set; }
    }

    /*The Owner class represents the information for each owner of a vehicle.*/
    public class Owner
    {
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("address")]
        public string Address { get; set; }

        [BsonElement("phone_number")]
        public string PhoneNumber { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("transfer_date")]
        public DateTime TransferDate { get; set; }
    }

    /*The VehicleService class provides the interface for registering a vehicle, updating its details, and transferring ownership.*/
    public class VehicleService
    {
        private IMongoCollection<Vehicle> _vehicles;

        public VehicleService(IMongoCollection<Vehicle> vehicles)
        {
            _vehicles = vehicles;
        }

        public Vehicle RegisterVehicle(Vehicle vehicle)
        {
            _vehicles.InsertOne(vehicle);
            return vehicle;
        }

        public void UpdateVehicle(string id, Vehicle vehicle)
        {
            _vehicles.ReplaceOne(v => v.Id == id, vehicle);
        }

        public void TransferOwnership(string id, Owner newOwner)
        {
            _vehicles.UpdateOne(v => v.Id == id, Builders<Vehicle>.Update.Push(v => v.Owners, newOwner));
        }
    }

    public class VehicleRepository
    {
        private IMongoCollection<Vehicle> _vehicles;

        public VehicleRepository(IMongoClient client, string databaseName, string collectionName)
        {
            var database = client.GetDatabase(databaseName);
            _vehicles = database.GetCollection<Vehicle>(collectionName);
        }

        public IMongoCollection<Vehicle> Vehicles
        {
            get
            {
                return _vehicles;
            }
        }
    }
}
