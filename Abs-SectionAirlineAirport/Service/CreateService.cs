using Abs.Common.Constants;
using Abs.Common.Constants.DbModels;
using ABS.Data.DynamoDbRepository;
using ABS_Common.Enumerations;
using ABS_Common.ResponsesModels;
using ABS_Data.Data;
using Abs_SectionAirlineAirport.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abs_SectionAirlineAirport.Service
{
    public class CreateService : ICreateService
    {

        IRepository<string, AirlineModel> _airlineRepository;
        IRepository<string, AirportModel> _airportRepository;
        IRepository<string, SectionModel> _sectionRepository;

        public CreateService(IRepository<string , AirlineModel> airlineRepository , IRepository<string , SectionModel> sectionRepository , IRepository<string , AirportModel> airportRepository)
        {
            _sectionRepository = sectionRepository;
            _airlineRepository = airlineRepository;
            _airportRepository = airportRepository;
        }

        public async Task<IActionResult> CreateAirline(AirlineModel airlineInfo)
        {
            await _airlineRepository.Add(airlineInfo);

            return new OkObjectResult(new ResponseObject("Airline created successfully"));
        }

        public async Task<IActionResult> CreateAirport(AirportModel airportInfo)
        {
            await _airportRepository.Add(airportInfo);

            return new OkObjectResult(new ResponseObject("Airport created successfully"));
        }

        public async Task<IActionResult> CreateSection(SectionBindingModel sectionInfo)
        {
            var sectionModel = new SectionModel()
            {
                Rows = sectionInfo.Rows,
                Columns = sectionInfo.Columns,
                AvailableSeats = sectionInfo.Rows * sectionInfo.Columns,
                SeatClass = sectionInfo.SeatClass.ToLower() == "first" ? SeatClass.First : sectionInfo.SeatClass.ToLower() == "bussiness" ? SeatClass.Bussiness : SeatClass.Economy,
                FlightNumber = sectionInfo.FlightNumber,
            };

            await _sectionRepository.Add(sectionModel);

            return new OkObjectResult(new ResponseObject("Section created"));
        }

    }
}
