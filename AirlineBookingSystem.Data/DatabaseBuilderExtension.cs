using AirlineBookingSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirlineBookingSystem.Data
{
    public static class DatabaseBuilderExtension
    {
        public static void SeedDataBase(this ModelBuilder builder)
        {

            var airlines = new Airline[] { new Airline() { Name = "DELTA", Id = -1, }, new Airline() { Name = "GAMA", Id = -2, }, new Airline() { Name = "GUL", Id = -3 } };


            var airports = new Airport[] { new Airport() { Name = "NYC", Id = -1 }, new Airport() { Name = "LAA", Id = -2 }, new Airport() { Name = "GTO", Id = -3 } };


            var flights = new Flight[] {
                new Flight()
                {
                    Id=-1,
                    FlightNumber="-123",
                    AirlineId = airlines[0].Id ,
                    OriginAirportId=airports[1].Id ,
                    DestinationAirportId =airports[0].Id,
                    DepartureDate=new DateTime(2025 , 7, 10) ,
                    LandingDate=new DateTime(2025 , 7 , 10)
                },
                new Flight()
                {
                    Id=-2,
                    FlightNumber="-1234",
                    AirlineId = airlines[1].Id ,
                    OriginAirportId=airports[0].Id ,
                    DestinationAirportId=airports[1].Id,
                    DepartureDate=new DateTime(2025 , 8, 10) ,
                    LandingDate=new DateTime(2025 , 8 , 10)
                },
                new Flight()
                {
                    Id=-3,
                    FlightNumber="-12345",
                    AirlineId = airlines[2].Id ,
                    OriginAirportId=airports[1].Id ,
                    DestinationAirportId=airports[0].Id,
                    DepartureDate=new DateTime(2025 , 7, 10) ,
                    LandingDate=new DateTime(2025 , 7 , 10)
                },
            };


            var sections = new Section[] {
                new Section()
                {
                    Id=-1,
                    Rows=1,
                    Columns=5,
                    SeatClass = SeatClass.First,
                    FlightId=flights[0].Id
                },
                new Section()
                {
                    Id=-2,
                    Rows=1,
                    Columns=5,
                    SeatClass = SeatClass.Bussiness,
                    FlightId=flights[1].Id,
                },
                new Section()
                {
                    Id=-3,
                    Rows=1,
                    Columns=5,
                    SeatClass = SeatClass.Economy,
                    FlightId=flights[2].Id
                },
            };

            var roles = new Role[]
            {
                new Role()
                {
                    Id=-1,
                    Name="Admin",
                    Authority=10
                },
                new Role()
                {
                    Id=-2,
                    Name="User",
                    Authority=1
                }
            };

            var users = new User[]
            {
                new User()
                {
                    Id=-1,
                    Username="admin",
                    Email="admin@admin.bg",
                    HashedPassword="admin",
                },
                new User()
                {
                    Id=-2,
                    Username="user1",
                    Email="user1@user.bg",
                    HashedPassword="user123",
                }
            };

            var userRoles = new UserRole[]
            {
                new UserRole()
                {
                    Id=-1,
                    UserId=users[0].Id,
                    RoleId = roles[0].Id,
                },
                new UserRole()
                {
                    Id=-2,
                    UserId=users[1].Id,
                    RoleId = roles[1].Id,
                }
            };


            builder.Entity<Role>()
                .HasData(roles);

            builder.Entity<Airline>()
                .HasData(airlines);

            builder.Entity<Airport>()
                .HasData(airports);

            builder.Entity<Flight>()
                .HasData(flights);

            builder.Entity<Section>()
                .HasData(sections);


            builder.Entity<UserRole>()
                .HasData(userRoles);

            builder.Entity<User>()
                .HasData(users);

        }
    }
}
