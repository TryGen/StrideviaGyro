using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
class DataIntake
{

    private string path = "C:\\Users\\user\\Desktop\\angle_test.csv";
    private List<string[]> csvData = new List<string[]>();
    public List<Quaternion> gyroData = new List<Quaternion>();

    public void getRawData()
    {
        readFromCSV();

        int col, sizeData = csvData.Count;

        for (int row = 1; row < sizeData; row++)
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

        for (int i = 1; i < 100; i++)
        {
            Console.Write(gyroData[i].x);
            Console.WriteLine();
        }

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