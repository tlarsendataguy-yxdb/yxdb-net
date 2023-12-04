using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace yxdb;

internal class GeoJson<T>
{
    public GeoJson(string objType, T objCoordinates)
    {
        Type = objType;
        Coordinates = objCoordinates;
    }

    [JsonPropertyName("type")]
    public string Type { get; set; } 
    
    [JsonPropertyName("coordinates")]
    public T Coordinates { get; set; }
}

/// <summary>
/// <para>
/// <c>Spatial</c> contains a static function to translate SpatialObj fields into GeoJSON.
/// </para>
/// </summary>
public static class Spatial
{
    private const int BytesPerPoint = 16;
    
    /// <summary>
    /// <para>
    /// <c>ToGeoJson</c> translates SpatialObj fields into GeoJSON.
    /// </para>
    /// <para>
    /// Alteryx stores spatial objects in a binary format. This function reads the binary format and converts
    /// it to a GeoJSON string.
    /// </para>
    /// <para>
    /// Throws a <c>FormatException</c> if the value provided is not a valid spatial object.
    /// </para>
    /// </summary>
    public static string ToGeoJson(byte[] value)
    {
        if (value == null)
        {
            return "";
        }

        if (value.Length < 20)
        {
            throw new FormatException("blob is not a spatial object");
        }

        var objType = LittleEndian.ToInt32(value, 0);
        switch (objType)
        {
            case 8:
                return ParsePoints(value);
            case 3:
                return ParseLines(value);
            case 5:
                return ParsePoly(value);
        }

        throw new FormatException("blob is not a spatial object");
    }

    private static string ParsePoints(byte[] value)
    {
        var totalPoints = LittleEndian.ToInt32(value, 36);
        if (totalPoints == 1)
        {
            return ParseSinglePoint(value);
        }

        return ParseMultiPoint(totalPoints, value);
    }

    private static string ParseSinglePoint(byte[] value)
    {
        return GeoJson("Point", GetCoordAt(value, 40));
    }

    private static string ParseMultiPoint(int totalPoints, byte[] value)
    {
        var points = new List<List<double>>(totalPoints);
        var i = 40;
        while (i < value.Length)
        {
            points.Add(GetCoordAt(value, i));
            i += BytesPerPoint;
        }

        return GeoJson("MultiPoint", points);
    }

    private static string ParseLines(byte[] value)
    {
        var lines = ParseMultiPointObject(value);

        if (lines.Count == 1)
        {
            return GeoJson("LineString", lines[0]);
        }

        return GeoJson("MultiLineString", lines);
    }

    private static string ParsePoly(byte[] value)
    {
        var poly = ParseMultiPointObject(value);

        if (poly.Count == 1)
        {
            return GeoJson("Polygon", poly);
        }

        return GeoJson("MultiPolygon", new List<List<List<List<double>>>> { poly });
    }

    private static List<List<List<double>>> ParseMultiPointObject(byte[] value)
    {
        var endingIndices = GetEndingIndices(value);

        var i = 48 + (endingIndices.Length * 4) - 4;
        var objects = new List<List<List<double>>>(endingIndices.Length);
        foreach (var endingIndexLong in endingIndices)
        {
            var endingIndex = Convert.ToInt32(endingIndexLong);
            var line = new List<List<double>>((endingIndex - i) / BytesPerPoint);
            while (i < endingIndex)
            {
                line.Add(GetCoordAt(value, i));
                i += BytesPerPoint;
            }
            objects.Add(line);
        }

        return objects;
    }

    private static long[] GetEndingIndices(byte[] value)
    {
        var totalObjects = LittleEndian.ToInt32(value, 36);
        var totalPoints = LittleEndian.ToInt64(value, 40);
        var endingIndices = new long[totalObjects];

        var i = 48;
        var startAt = 48 + ((totalObjects - 1) * 4);
        for (var j = 1; j < totalObjects; j++)
        {
            var endingPoint = LittleEndian.ToInt32(value, i);
            var endingIndex = (endingPoint * BytesPerPoint) + startAt;
            endingIndices[j - 1] = endingIndex;
            i += 4;
        }

        endingIndices[totalObjects - 1] = (totalPoints * BytesPerPoint) + startAt;
        return endingIndices;
    }

    private static List<double> GetCoordAt(byte[] value, int startAt)
    {
        var lng = BitConverter.ToDouble(value, startAt);
        var lat = BitConverter.ToDouble(value, startAt + 8);
        return new List<double>{lng, lat};
    }

    private static string GeoJson<T>(string objType, T value)
    {
        var toSerialize = new GeoJson<T>(objType, value);
        return JsonSerializer.Serialize(toSerialize);
    }
}