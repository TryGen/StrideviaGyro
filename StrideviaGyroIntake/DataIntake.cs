using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
public class DataIntake
{

    private string path = "C:\\Users\\user\\Desktop\\angle_test.csv";
    private List<string[]> csvData = new List<string[]>();
    public static List<Quaternion> gyroData = new List<Quaternion>();

    /// <summary>
    /// The time it takes the gyroscope to calibrate
    /// </summary>
    private int calibrationTime = 190;

    public static int size;

    public void getRawData()
    {
        readFromCSV();

        int col, sizeData = csvData.Count;

        for (int row = calibrationTime; row < sizeData; row++)
        {
            col = 7;

            while (col < 11)
            {
                /*  gyroData.Add(new Quaternion(
                      float.Parse(csvData[row][col++]),
                      float.Parse(csvData[row][col++]),
                      float.Parse(csvData[row][col++]),
                      float.Parse(csvData[row][col++])
                      ));*/

                gyroData.Add(new Quaternion(
                    double.Parse(csvData[row][col++], CultureInfo.InvariantCulture),
                    double.Parse(csvData[row][col++], CultureInfo.InvariantCulture),
                    double.Parse(csvData[row][col++], CultureInfo.InvariantCulture),
                    double.Parse(csvData[row][col++], CultureInfo.InvariantCulture)

                    ));
            }
        }

        size = gyroData.Count;
       

    }

    private void readFromCSV()
    {

        StreamReader reader = new StreamReader(path);

        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            string[] values = line.Split(',');
            csvData.Add(values);
        }


    }


}