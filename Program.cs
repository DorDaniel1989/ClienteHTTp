using System;

using System.Net.Http;
using System.Net.Http.Headers;
using NW = Newtonsoft.Json;
using MS = System.Text.Json;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

class Program
{
    static async Task Main(string[] args)
    {

        TimeSpan interval = new TimeSpan(0, 0, 4000);
        try
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("COMIENZA LA ACTUALIZACION DE LA BASE DE DATOS");

        }
        catch (Exception a)
        {
            Console.WriteLine("error=>" + a);
        }
        while (true)
        {
            await TareaAsincrona();
            Thread.Sleep(interval);
        }



    }

    static async Task TareaAsincrona()
    {
        var client = new HttpClient();
        var key = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiJtZXQwMS5hcGlrZXkiLCJpc3MiOiJJRVMgUExBSUFVTkRJIEJISSBJUlVOIiwiZXhwIjoyMjM4MTMxMDAyLCJ2ZXJzaW9uIjoiMS4wLjAiLCJpYXQiOjE2NDE5NzM4MDcsImVtYWlsIjoiaWtiZHZAcGxhaWF1bmRpLm5ldCJ9.IofLYTTBr0PZoiLxmVzrqBU6vYWnoQX8Bi2SorSrvnzinBIG28AutQL3M6CEvLWstteyX74gQzCltKxZYrWUYkrsi9GXWsMzz20TiiSkz1D2KarxLiV5a4yFN71NybjYG_XHEWmnkoMIZmlFQ6O3f4ixyFdSFmLEVjI1-2Ud4XD8LNm035o_8_kkFxKYLYhElnn8wwC44tt5CeT9efMOxQLKa9JrsHUMapypWOybXIeSyScRAgjN8dMySX6IZx7YX6Wt3-buzFxXmBQAlmjvNULWQ0r2VPHnthETr72RWLT1hYhXxOaLdBEnGe6F7hiwTHonU9fy_wBkr2i697qGTA";
        client.DefaultRequestHeaders.Add("User-Agent", "mi consola");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + key);
        //Hacemos la primera peticion a la API para recibir las regiones
        var urlRegiones = $"https://api.euskadi.eus/euskalmet/geo/regions/basque_country/zones";
        //Esperamos a recibir la respuesta de la API
        HttpResponseMessage respuestaRegiones = await client.GetAsync(urlRegiones);
        //Esperamos a recibir el contenido de la respuesta
        var sRespRegiones = await respuestaRegiones.Content.ReadAsStringAsync();
        //Parseamos la respuesta de string a objeto json
        dynamic jsonObjectRegiones = NW.JsonConvert.DeserializeObject(sRespRegiones);
        //Limpiamos lo que haya en la consola para arrancar de 0
        Console.Clear();






        //Guardamos valores de la fecha actual del sistema
        var diaHoy = DateTime.Today.Day;
        var AñoHoy = DateTime.Today.Year;
        var mesHoy = DateTime.Today.Month;
        var hora = DateTime.Now.Hour;

        string urlLocalidades2;
        HttpResponseMessage respuestaLocalidades2;
        dynamic jsonObjectLocalidades2;


        foreach (var item in jsonObjectRegiones)
        {

            urlLocalidades2 = $"https://api.euskadi.eus/euskalmet/geo/regions/basque_country/zones/{item.regionZoneId}/locations";
            respuestaLocalidades2 = await client.GetAsync(urlLocalidades2);
            var sRespLocalidades2 = await respuestaLocalidades2.Content.ReadAsStringAsync();
            //Parseamos la respuesta de string a objeto json
            jsonObjectLocalidades2 = NW.JsonConvert.DeserializeObject(sRespLocalidades2);
            //Console.WriteLine(jsonObjectLocalidades2);
            foreach (var item2 in jsonObjectLocalidades2)
            {
                using (var db = new WebApiTiempoContext())
                {
                    try
                    {
                        if (hora == 0) hora = 24;// La hora del Pc marca 0 cuando son las 12 de la noche, asi se corrige el error que produciria
                        string sHora;
                        //Si es antes de las 10 de la mañana agregamos
                        if (hora < 11)
                        {
                            sHora = "0" + (hora - 1);
                        }
                        else
                        {
                            sHora = (hora - 1).ToString();
                        }
                        var urlLocalizacionForecast1 = $"https://api.euskadi.eus/euskalmet/weather/regions/basque_country/zones/{item.regionZoneId}/locations/{item2.regionZoneLocationId}/forecast/trends/measures/at/{AñoHoy}/0{mesHoy}/{diaHoy}/for/{AñoHoy}0{mesHoy}{diaHoy}";
                        HttpResponseMessage respuestaRegistrosDeTiempo1 = await client.GetAsync(urlLocalizacionForecast1);
                        var sRespRegistrosDeTiempo1 = await respuestaRegistrosDeTiempo1.Content.ReadAsStringAsync();
                        //Este objeto contiene 24 registros diferentes, uno por cada hora
                        dynamic jsonObjectRegistrosDeTiempo1 = NW.JsonConvert.DeserializeObject(sRespRegistrosDeTiempo1);

                        int ultimoRegistro = 0;
                        //En este bucle buscamos el registro mas actual basada en la hora del pc
                        for (var x = 0; x < jsonObjectRegistrosDeTiempo1.trends.set.Count; x++)
                        {

                            //Si el string del apartado range del registro que estamos iterando contiene los digitos buscados, lo hemos encontrado
                            if (((jsonObjectRegistrosDeTiempo1.trends.set[x].range)).ToString().Contains(sHora))
                            {
                                ultimoRegistro = x; //Guardamos la posicion de la iteracion, ya que coincidira con la posicion del registro en el jsonObjectRegistrosDeTiempo

                            }
                        }

                        dynamic temp = jsonObjectRegistrosDeTiempo1.trends.set[ultimoRegistro].temperature;
                        dynamic prec = jsonObjectRegistrosDeTiempo1.trends.set[ultimoRegistro].precipitation;
                        dynamic vvi = jsonObjectRegistrosDeTiempo1.trends.set[ultimoRegistro].windspeed;
                        dynamic desc = jsonObjectRegistrosDeTiempo1.trends.set[ultimoRegistro].symbolSet.weather.nameByLang.SPANISH;
                        dynamic pathImg = jsonObjectRegistrosDeTiempo1.trends.set[ultimoRegistro].symbolSet.weather.path;
                        dynamic rangHor = jsonObjectRegistrosDeTiempo1.trends.set[ultimoRegistro].range;
                        //PARA METER LOS DATOS DESDE 0
                        //var a1 = new TiempoItem { 
                        //    Municipio = $"{item2.regionZoneLocationId}",
                        //    Region = $"{item.regionZoneId}",
                        //    Fecha = $"{diaHoy}/{mesHoy}/{AñoHoy} : {sHora}",
                        //    Temperatura = $"{(temp.value).ToString()}",
                        //    VelocidadViento = $"{(vvi.value).ToString()}",
                        //    PrecipitacionAcumulada = $"{(prec.value).ToString()}",
                        //    Descripcion = $"{(desc).ToString()}",
                        //    PathImg = $"{(pathImg).ToString()}"

                        //};

                        string localizacion = item2.regionZoneLocationId + "";
                        var row = db.TiempoItem.Where(a => a.Municipio == localizacion).Single();
                        row.Fecha = jsonObjectRegistrosDeTiempo1.trends.set[ultimoRegistro].range;
                        row.VelocidadViento = vvi.value;
                        row.Temperatura = temp.value;
                        row.PrecipitacionAcumulada = prec.value;
                        row.Descripcion = desc;
                        row.PathImg = jsonObjectRegistrosDeTiempo1.trends.set[ultimoRegistro].symbolSet.weather.path;
                        db.SaveChanges();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("");
                        Console.WriteLine("El registro se actualizo correctamente :) ");
                        // db.TiempoItem.Add(a1);
                        // db.SaveChanges();
                    }
                    catch (Exception p)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("");
                        Console.Write("No existe ningún registro ... :(");
                    }

                }

            }

        }

    }

    //Configuracion de del cliente para conectarse a Euskalmet
}


//var bucle = true;
//while (bucle)
//{
//    //Modificacion de el color de las letras de consola
//    Console.ForegroundColor = ConsoleColor.Blue;
//    Console.WriteLine("     # SELECCIONA UNA REGION");
//    //Imprimimos lo que tenemos dentro de el objeto que contiene las Regiones recibidas
//    foreach (var item in jsonObjectRegiones)
//    {
//        Console.ForegroundColor = ConsoleColor.Yellow;
//        Console.WriteLine($"         {item.regionZoneId}");
//    }

//    Console.ForegroundColor = ConsoleColor.White;
//    Console.WriteLine("**********************************");
//    Console.WriteLine("Introduce la zona....");
//    var zona = Console.ReadLine();
//    Console.WriteLine("**********************************");
//    Console.ForegroundColor = ConsoleColor.Blue;
//    Console.WriteLine("     # LOCALIDADES EN LA REGION SELECCIONADA");
//    //Construimos la url con la zona que ha introducido el usuario
//    var urlLocalidades = $"https://api.euskadi.eus/euskalmet/geo/regions/basque_country/zones/{zona}/locations";
//    //Y hacemos la peticion como antes
//    HttpResponseMessage respuestaLocalidades = await client.GetAsync(urlLocalidades);
//    var sRespLocalidades = await respuestaLocalidades.Content.ReadAsStringAsync();
//    dynamic jsonObjectLocalidades = NW.JsonConvert.DeserializeObject(sRespLocalidades);

//    var i = 1; //Variable que numera la lista de localidades
//    string respuestaContinuar; //Variable que corta el programa

//    //Imprimimos las localidades asociadas a la region introducida por el usuario
//    foreach (var item in jsonObjectLocalidades)
//    {
//        Console.ForegroundColor = ConsoleColor.Yellow;
//         Console.WriteLine($"         {i}- {item.regionZoneLocationId}");
//        i++;
//    }

//    Console.ForegroundColor = ConsoleColor.White;
//    Console.WriteLine("**********************************");
//    Console.WriteLine("Introduce la localidad....");
//    var municipio = Console.ReadLine();
//    //Formamos la url de la peticion que nos devuelve los registros en Euskalmet con la informacion del tiempo para la localidad introducida por el usuario
//    var urlLocalizacionForecast = $"https://api.euskadi.eus/euskalmet/weather/regions/basque_country/zones/{zona}/locations/{municipio}/forecast/trends/measures/at/{AñoHoy}/0{mesHoy}/{diaHoy}/for/{AñoHoy}0{mesHoy}{diaHoy}";
//    HttpResponseMessage respuestaRegistrosDeTiempo = await client.GetAsync(urlLocalizacionForecast);
//    var sRespRegistrosDeTiempo = await respuestaRegistrosDeTiempo.Content.ReadAsStringAsync();
//    //Este objeto contiene 24 registros diferentes, uno por cada hora
//    dynamic jsonObjectRegistrosDeTiempo = NW.JsonConvert.DeserializeObject(sRespRegistrosDeTiempo);
//    try
//    {
//        if (hora == 0) hora = 24;// La hora del Pc marca 0 cuando son las 12 de la noche, asi se corrige el error que produciria
//        string sHora;
//        //Si es antes de las 10 de la mañana agregamos
//        if (hora < 11)
//        {
//            sHora = "0" + (hora - 1);
//        }
//        else
//        {
//            sHora = (hora - 1).ToString();
//        }
//        Console.WriteLine($"Iterando sobre los {jsonObjectRegistrosDeTiempo.trends.set.Count} registros para buscar el mas cercano a la hora actual....");
//        int ultimoRegistro = 0;
//        //En este bucle buscamos el registro mas actual basada en la hora del pc
//        for (var x = 0; x < jsonObjectRegistrosDeTiempo.trends.set.Count; x++)
//        {

//            Console.ForegroundColor = ConsoleColor.DarkMagenta;
//            Console.WriteLine($"{(jsonObjectRegistrosDeTiempo.trends.set[x].range).ToString()} ");
//            //Si el string del apartado range del registro que estamos iterando contiene los digitos buscados, lo hemos encontrado
//            if (((jsonObjectRegistrosDeTiempo.trends.set[x].range)).ToString().Contains(sHora))
//            {
//                Console.ForegroundColor = ConsoleColor.DarkYellow;
//                Console.WriteLine($"EL ULTIMO REGISTRO DEL DIA ES =>>>> {jsonObjectRegistrosDeTiempo.trends.set[x].range} ");
//                ultimoRegistro = x; //Guardamos la posicion de la iteracion, ya que coincidira con la posicion del registro en el jsonObjectRegistrosDeTiempo
//                Console.ForegroundColor = ConsoleColor.DarkMagenta;
//            }

//        }
//        //Recuperamos los valores del registro deseado
//        dynamic temp = jsonObjectRegistrosDeTiempo.trends.set[ultimoRegistro].temperature;
//        dynamic prec = jsonObjectRegistrosDeTiempo.trends.set[ultimoRegistro].precipitation;
//        dynamic vvi = jsonObjectRegistrosDeTiempo.trends.set[ultimoRegistro].windspeed;
//        dynamic desc = jsonObjectRegistrosDeTiempo.trends.set[ultimoRegistro].symbolSet.weather.nameByLang.SPANISH;
//        dynamic pathImg = jsonObjectRegistrosDeTiempo.trends.set[ultimoRegistro].symbolSet.weather.path;
//        dynamic rangHor = jsonObjectRegistrosDeTiempo.trends.set[ultimoRegistro].range;

   
//        //Imprimimos los resultados con colorines
//        Console.ForegroundColor = ConsoleColor.Blue;
//        Console.WriteLine($" INFORMACION DEL TIEMPO EN {municipio.ToUpper()} a fecha {AñoHoy} / 0{mesHoy} / {diaHoy} Hora : {hora - 1}");
//        Console.ForegroundColor = ConsoleColor.DarkYellow;
//        Console.Write($"Temperatura  =");
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.Write($" {temp.value} Cº ");
//        Console.WriteLine();
//        Console.ForegroundColor = ConsoleColor.DarkYellow;
//        Console.Write($"Precitipacion acumulada  =");
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.Write($" {prec.value} ml ");
//        Console.WriteLine();
//        Console.ForegroundColor = ConsoleColor.DarkYellow;
//        Console.Write($"Velocidad del Viento  =");
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.Write($" {vvi.value} Km/h  ");
//        Console.WriteLine();
//        Console.ForegroundColor = ConsoleColor.DarkYellow;
//        Console.Write($"Descripcion  =");
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.Write($" {desc}  ");
//        Console.WriteLine();
//        Console.ForegroundColor = ConsoleColor.DarkYellow;
//        Console.Write($"Ruta de la imagen  =");
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.Write($"  {pathImg}  ");
//        Console.WriteLine();
//        Console.ForegroundColor = ConsoleColor.DarkYellow;
//        Console.Write($"Rango horario  =");
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.Write($"  {rangHor}  ");
//        Console.WriteLine();
//        Console.ForegroundColor = ConsoleColor.White;
//        //Preguntamos al usuario si quiere continuar
//        Console.WriteLine("====");
//        Console.WriteLine("Presiona N para salir, para continuar introduce cualquier otra tecla...");
//        respuestaContinuar = Console.ReadLine();
//        if (respuestaContinuar.Equals("N"))
//        {
//            bucle = false;
//        }
//        Console.Clear();
//    }
//    //Saltara excepcion si no se recibe una respuesta en la API, y nos avisara de que no existe ese dato en Euskalmet
//    catch (Exception e)
//    {
//        Console.ForegroundColor = ConsoleColor.Red;
//        Console.WriteLine("Lo siento no hay datos en Euskalmet");
//        Console.WriteLine("Presiona N para salir, para continuar introduce cualquier otra tecla...");
//        respuestaContinuar = Console.ReadLine();
//        if (respuestaContinuar.Equals("N"))
//        {
//            bucle = false;
//        }
//        Console.ForegroundColor = ConsoleColor.White;
//        Console.Clear();
//    }

//}




