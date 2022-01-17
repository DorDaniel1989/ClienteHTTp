﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using NW = Newtonsoft.Json;
using MS = System.Text.Json;
using System.Collections.Generic;

using var client = new HttpClient();
var key = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiJtZXQwMS5hcGlrZXkiLCJpc3MiOiJJRVMgUExBSUFVTkRJIEJISSBJUlVOIiwiZXhwIjoyMjM4MTMxMDAyLCJ2ZXJzaW9uIjoiMS4wLjAiLCJpYXQiOjE2NDE5NzM4MDcsImVtYWlsIjoiaWtiZHZAcGxhaWF1bmRpLm5ldCJ9.IofLYTTBr0PZoiLxmVzrqBU6vYWnoQX8Bi2SorSrvnzinBIG28AutQL3M6CEvLWstteyX74gQzCltKxZYrWUYkrsi9GXWsMzz20TiiSkz1D2KarxLiV5a4yFN71NybjYG_XHEWmnkoMIZmlFQ6O3f4ixyFdSFmLEVjI1-2Ud4XD8LNm035o_8_kkFxKYLYhElnn8wwC44tt5CeT9efMOxQLKa9JrsHUMapypWOybXIeSyScRAgjN8dMySX6IZx7YX6Wt3-buzFxXmBQAlmjvNULWQ0r2VPHnthETr72RWLT1hYhXxOaLdBEnGe6F7hiwTHonU9fy_wBkr2i697qGTA";
client.DefaultRequestHeaders.Add("User-Agent", "mi consola");
client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
client.DefaultRequestHeaders.Add("Authorization", "Bearer " + key);


// Parse JSON response.
var urlLocalizacionForecast= $"https://api.euskadi.eus/euskalmet/weather/regions/basque_country/zones/great_bilbao/locations/bilbao/forecast/trends/measures/at/2020/11/27/for/20201127";

var urlEstaciones = "https://api.euskadi.eus/euskalmet/stations";
var url = "https://api.euskadi.eus/euskalmet/readings/forStation/C071/R0BU/measures/measuresForAir/temperature/at/2022/01/14/08";
HttpResponseMessage response = await client.GetAsync(url);
HttpResponseMessage response2 = await client.GetAsync(urlEstaciones);
HttpResponseMessage response3 = await client.GetAsync(urlLocalizacionForecast);

response.EnsureSuccessStatusCode();
var resp = await response.Content.ReadAsStringAsync();
var resp2 = await response2.Content.ReadAsStringAsync();
var resp3 = await response3.Content.ReadAsStringAsync();

//Root myDeserializedClass = NW.JsonConvert.DeserializeObject<Root>(resp);
//Root2 myDeserializedClass2 = NW.JsonConvert.DeserializeObject<Root2>(resp2);

//foreach (var item in myDeserializedClass.values)
//    Console.WriteLine($" {item} ");

//Console.WriteLine("====");
dynamic jsonObject2 = NW.JsonConvert.DeserializeObject(resp2);


foreach (var item in jsonObject2)
    Console.WriteLine($" {item.stationId} ");

//Root4 myDeserializedClass3 = NW.JsonConvert.DeserializeObject<Root4>(resp3);
dynamic jsonObject3 = NW.JsonConvert.DeserializeObject(resp3);

var pp =  jsonObject3.trends.set[0].temperature;
var pp1 = jsonObject3.trends.set[0].precipitation;
var pp2 = jsonObject3.trends.set[0].windspeed;
Console.WriteLine($" {pp} ");

foreach (dynamic item in pp)
{

    Console.WriteLine($" Temperatura  {item.First} ");
}

foreach (dynamic item in pp1)
{
   
    Console.WriteLine($" Precipitacion {item.First} ");
}

foreach (dynamic item in pp2)
{

    Console.WriteLine($" Velocidad del viento {item.First} ");
}

Fuente: https://www.iteramos.com/pregunta/55743/obtener-la-lista-de-valores-distintos-en-la-listafoo
Console.WriteLine("====");

dynamic jsonObject = NW.JsonConvert.DeserializeObject(resp);
Console.WriteLine(jsonObject.values);

//**************************************************************//
public class Temperature
{
    public double value { get; set; }
    public string unit { get; set; }
}

public class Precipitation
{
    public int value { get; set; }
    public string unit { get; set; }
}

public class Winddirection
{
    public int value { get; set; }
    public string unit { get; set; }
    public string cardinalpoint { get; set; }
}

public class Windspeed
{
    public double value { get; set; }
    public string unit { get; set; }
}

public class NameByLang
{
    public string SPANISH { get; set; }
    public string BASQUE { get; set; }
}

public class DescriptionByLang
{
    public string SPANISH { get; set; }
    public string BASQUE { get; set; }
}

public class Weather
{
    public string id { get; set; }
    public string path { get; set; }
    public NameByLang nameByLang { get; set; }
    public DescriptionByLang descriptionByLang { get; set; }
}

public class SymbolSet
{
    public Weather weather { get; set; }
}

public class ShortDescription
{
    public string SPANISH { get; set; }
    public string BASQUE { get; set; }
}

public class Set
{
    public string range { get; set; }
    public Temperature temperature { get; set; }
    public Precipitation precipitation { get; set; }
    public Winddirection winddirection { get; set; }
    public Windspeed windspeed { get; set; }
    public SymbolSet symbolSet { get; set; }
    public ShortDescription shortDescription { get; set; }
}

public class Trends
{
    public List<Set> set { get; set; }
}

public class Root4
{
    public string oid { get; set; }
    public int numericId { get; set; }
    public int entityVersion { get; set; }
    public DateTime at { get; set; }
    public DateTime @for { get; set; }
    public Trends trends { get; set; }
}





//***********************************************************//
public class Slot
{
    public string range { get; set; }
    public string rangeDesc { get; set; }
    public string lowerEndPointDesc { get; set; }
    public string upperEndPointDesc { get; set; }
}

public class Root3
{
    public string typeId { get; set; }
    public string oid { get; set; }
    public int numericId { get; set; }
    public int entityVersion { get; set; }
    public string station { get; set; }
    public string sensor { get; set; }
    public string measureType { get; set; }
    public string measure { get; set; }
    public string dateRange { get; set; }
    public List<Slot> slots { get; set; }
    public List<double> values { get; set; }
}


public class Root2
{
    public string key { get; set; }
    public string stationId { get; set; }
    public DateTime snapshotDate { get; set; }
}

