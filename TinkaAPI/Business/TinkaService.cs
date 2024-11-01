using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using MathNet.Numerics.LinearRegression;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class TinkaService
{
    private readonly Container _container;

    public TinkaService(Container container)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
    }

    public async Task<ItemResponse<Sorteo>> CreateSorteoAsync(Sorteo sorteo)
    {        
        return await _container.CreateItemAsync(sorteo, new PartitionKey(sorteo.Id));
    }

    public async Task<ItemResponse<FrecuenciaBolilla>> CreateFrecuenciaAsync(FrecuenciaBolilla frecuencia)
    {
        return await _container.CreateItemAsync(frecuencia, new PartitionKey(frecuencia.Id));
    }

    public async Task DeleteAllSorteosAsync()
    {
        var query = "SELECT * FROM c WHERE c.documentType = 'Sorteo'";
        var iterator = _container.GetItemQueryIterator<Sorteo>(new QueryDefinition(query));
        while (iterator.HasMoreResults)
        {
            foreach (var sorteo in await iterator.ReadNextAsync())
            {
                await _container.DeleteItemAsync<Sorteo>(sorteo.Id, new PartitionKey(sorteo.Id));
            }
        }
    }

    public async Task DeleteSorteoAsync(string id)
    {
        await _container.DeleteItemAsync<Sorteo>(id, new PartitionKey(id));
    }

    public async Task DeleteAllFrecuenciasAsync()
    {
        var query = "SELECT * FROM c WHERE c.documentType = 'FrecuenciaBolilla'";
        var iterator = _container.GetItemQueryIterator<FrecuenciaBolilla>(new QueryDefinition(query));
        while (iterator.HasMoreResults)
        {
            foreach (var frecuencia in await iterator.ReadNextAsync())
            {
                await _container.DeleteItemAsync<FrecuenciaBolilla>(frecuencia.Id, new PartitionKey(frecuencia.Id));
            }
        }
    }

    public async Task DeleteFrecuenciaAsync(int bolilla)
    {
        await _container.DeleteItemAsync<FrecuenciaBolilla>(bolilla.ToString(), new PartitionKey(bolilla.ToString()));
    }

    public async Task<List<Sorteo>> GetAllSorteosAsync()
    {
        var query = "SELECT * FROM c WHERE c.documentType = 'Sorteo'";
        var iterator = _container.GetItemQueryIterator<Sorteo>(new QueryDefinition(query));
        var sorteos = new List<Sorteo>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            sorteos.AddRange(response);
        }
        return sorteos;
    }

    public async Task<List<FrecuenciaBolilla>> GetAllFrecuenciasAsync()
    {
        var query = "SELECT * FROM c WHERE c.documentType = 'FrecuenciaBolilla'";
        var iterator = _container.GetItemQueryIterator<FrecuenciaBolilla>(new QueryDefinition(query));
        var frecuencias = new List<FrecuenciaBolilla>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            frecuencias.AddRange(response);
        }
        return frecuencias;
    }

    // Métodos de mayor y menor frecuencia, regresión polinómica, etc.
    public int[] MayorFrecuencia(List<Sorteo> sorteos)
    {
        Dictionary<int, int> frecuencia = new Dictionary<int, int>();

        foreach (var sorteo in sorteos)
        {
            int[] bolillas = { sorteo.Bolilla1, sorteo.Bolilla2, sorteo.Bolilla3, sorteo.Bolilla4, sorteo.Bolilla5, sorteo.Bolilla6 };

            foreach (var bolilla in bolillas)
            {
                if (frecuencia.ContainsKey(bolilla))
                {
                    frecuencia[bolilla]++;
                }
                else
                {
                    frecuencia[bolilla] = 1;
                }
            }
        }

        var numerosOrdenados = frecuencia.OrderByDescending(x => x.Value).Take(6);
        int[] arrResult = new int[6];
        int colResult = 0;

        foreach (var numero in numerosOrdenados)
        {
            arrResult[colResult] = numero.Key;
            colResult++;
        }

        return arrResult;
    }

    public int[] MenorFrecuencia(List<Sorteo> sorteos)
    {
        Dictionary<int, int> frecuencia = new Dictionary<int, int>();

        foreach (var sorteo in sorteos)
        {
            int[] bolillas = { sorteo.Bolilla1, sorteo.Bolilla2, sorteo.Bolilla3, sorteo.Bolilla4, sorteo.Bolilla5, sorteo.Bolilla6 };

            foreach (var bolilla in bolillas)
            {
                if (frecuencia.ContainsKey(bolilla))
                {
                    frecuencia[bolilla]++;
                }
                else
                {
                    frecuencia[bolilla] = 1;
                }
            }
        }

        var numerosOrdenados = frecuencia.OrderBy(x => x.Value).Take(6);
        int[] arrResult = new int[6];
        int colResult = 0;

        foreach (var numero in numerosOrdenados)
        {
            arrResult[colResult] = numero.Key;
            colResult++;
        }

        return arrResult;
    }

    public int[] RegresionPolinomica(List<Sorteo> sorteos)
    {
        var bolillas = new List<double>();
        var indices = new List<double>();

        foreach (var sorteo in sorteos)
        {
            bolillas.AddRange(new double[] { sorteo.Bolilla1, sorteo.Bolilla2, sorteo.Bolilla3, sorteo.Bolilla4, sorteo.Bolilla5, sorteo.Bolilla6 });
            indices.AddRange(Enumerable.Repeat(bolillas.Count / 6.0, 6));
        }

        var X = DenseMatrix.OfColumns(bolillas.Count, 2, new[] {
            Enumerable.Repeat(1.0, bolillas.Count),
            bolillas
        });

        var y = Vector<double>.Build.Dense(bolillas.ToArray());

        // Calcular coeficientes de la regresión
        var p = MultipleRegression.QR(X, y);

        // Generar las predicciones
        var predicciones = new List<int>();
        for (int i = 1; i <= 6; i++)
        {
            var prediccion = p[0] + p[1] * i;
            predicciones.Add((int)Math.Round(prediccion));
        }

        return predicciones.ToArray();
    }

    public int[] PrediccionFinal()
    {
        var sorteos = GetAllSorteosAsync().Result;

        var mayorFrecuencia = MayorFrecuencia(sorteos);
        var menorFrecuencia = MenorFrecuencia(sorteos);
        var regresionPolinomica = RegresionPolinomica(sorteos);

        // Combinación y selección aleatoria de 6 números de las listas anteriores
        var combinacion = mayorFrecuencia.Concat(menorFrecuencia).Concat(regresionPolinomica)
                                         .OrderBy(x => Guid.NewGuid())
                                         .Take(6)
                                         .ToArray();

        return combinacion;
    }
}

public class Sorteo 
{
    [JsonProperty("id")]
    public string Id { get; set; }
    public DateTime Fecha { get; set; }
    public int Bolilla1 { get; set; }
    public int Bolilla2 { get; set; }
    public int Bolilla3 { get; set; }
    public int Bolilla4 { get; set; }
    public int Bolilla5 { get; set; }
    public int Bolilla6 { get; set; }
    public string documentType { get; set; } = "Sorteo";
}

public class FrecuenciaBolilla 
{
    [JsonProperty("id")]
    public string Id => Bolilla.ToString();
    public int Bolilla { get; set; }
    public int NumVeces { get; set; }
    public string documentType { get; set; } = "FrecuenciaBolilla";
}


