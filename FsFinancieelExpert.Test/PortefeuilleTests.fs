
namespace FsFinancieelExpert.Test
open System
open System.IO
open Xunit
open FundReader.XmlFundLoader

module XmlFundLoaderTests =
    let sampleXml =
        """<?xml version="1.0" encoding="UTF-8"?>
        <funds>
          <fund>
            <Provider>TestBank</Provider>
            <Name>Test Fund A</Name>
            <Description>desc A</Description>
            <TimeSlice>
              <Date>2025-07-01</Date>
              <HowMany>10.0</HowMany>
              <Price>2.5</Price>
            </TimeSlice>
            <TimeSlice>
              <Date>2025-08-01</Date>
              <HowMany>5.0</HowMany>
              <Price>3.5</Price>
            </TimeSlice>
          </fund>
          <fund>
            <Provider>OtherBank</Provider>
            <Name>Test Fund B</Name>
            <Description>desc B</Description>
            <TimeSlice>
              <Date>2025-08-31</Date>
              <HowMany>2.0</HowMany>
              <Price>120.0</Price>
            </TimeSlice>
          </fund>
		  <fund>
		    <Provider>ASN Bank</Provider>
		    <Name>ASN Aandelenfonds</Name>
		    <Description>ASN Aandelenfonds (Duurzaam fonds)</Description>
		    <TimeSlice>
			  <Date>2025-07-31</Date>
			  <HowMany>3.0</HowMany>
			  <Price>160.5</Price>
		    </TimeSlice>
		    <TimeSlice>
			  <Date>2025-08-31</Date>
			  <HowMany>2.0</HowMany>
			  <Price>150.5</Price>
		    </TimeSlice>
	      </fund>
        </funds>
        """

    let writeTempXml (content: string) =
        let path = Path.GetTempFileName() + ".xml"
        File.WriteAllText(path, content)
        path

    [<Fact>]
    let ``LoadFundsChooseLatestAsOfDate computes totals correctly for current date`` () =
        let path = writeTempXml sampleXml
        try
            let repo = LoadFundsChooseLatestAsOfDate(path, DateTime.Now)
            Assert.Equal(3, repo.Funds.Length)
            let fA = repo.Funds |> List.find (fun f -> f.Name = "Test Fund A")
            Assert.Equal(17.50M, fA.TotalValue)
            let fB = repo.Funds |> List.find (fun f -> f.Name = "Test Fund B")
            Assert.Equal(240.0M, fB.TotalValue)
            let fC = repo.Funds |> List.find (fun f -> f.Name = "ASN Aandelenfonds")
            Assert.Equal(301.0M, fC.TotalValue)
        finally
            File.Delete(path)

    [<Fact>]
    let ``LoadFundsChooseLatestAsOfDate returns only latest slice before August 1st, 2025`` () =
        let path = writeTempXml sampleXml
        try
            let repo = LoadFundsChooseLatestAsOfDate(path, new DateTime(2025, 8, 1))
            let fA = repo.Funds |> List.find (fun f -> f.Name = "ASN Aandelenfonds")
            Assert.Equal(1, fA.TimeSlices.Length)
            Assert.Equal(481.50M, fA.TotalValue)
        finally
            File.Delete(path)
