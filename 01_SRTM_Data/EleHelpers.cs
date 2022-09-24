using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SC._01_SRTM_Data
{
    class EleHelpers
    {
        public static double getmaxEle(List<TrackPoint> _AktiveTrack)
        {
            double max_Ele = Convert.ToDouble(_AktiveTrack[0].ele);
                for (int k = 0; k < _AktiveTrack.Count; k++)
                {
                    if (Convert.ToDouble(_AktiveTrack[k].ele) > max_Ele)
                        max_Ele = Convert.ToDouble(_AktiveTrack[k].ele);
                }
            return max_Ele;
        }

        public static double getminEle(List<TrackPoint> _AktiveTrack)
        {
            double min_Ele = Convert.ToDouble(_AktiveTrack[0].ele);
            for (int k = 0; k < _AktiveTrack.Count; k++)
                {
                    if (Convert.ToDouble(_AktiveTrack[k].ele) < min_Ele)
                        min_Ele = Convert.ToDouble(_AktiveTrack[k].ele);
                }
            return min_Ele;
        }

        public static List<double[]> GetEleData(List<TrackPoint> Input)
        {
            List<TrackPoint> _Input = Input;
            List<double[]> _Output = new List<double[]>();



            if (Input.Any()) //prevent IndexOutOfRangeException for empty list - code snippet from code.grepper.com
            {
                for (int i = 0; i < _Input.Count - 2; i++) // skip last trackpoint -> no elevation information
                {
                    if (i > 0)
                    {
                        if ((Input[i].dist - Input[i - 1].dist) < 1000)
                        {
                            double[] newElement = new double[2] { (double)_Input[i].ele, 1 };
                            // 2 value array: first element = ele, second element = is element valid, see below
                            // check for distance between the track points: if distance >1000m second value will be 0, else 1

                            _Output.Add(newElement);
                        }
                        else
                        {
                            double[] newElement = new double[2] { (double)_Input[i].ele, 0 };
                            // ele-value is NOT valid (due to large distance between the track points
                            _Output.Add(newElement);
                        }
                    }
                }
            }
            else
            {
                double[] newElement1 = new double[2] { 0, 1 };
                _Output.Add(newElement1);
            }
            return Check4NonValidValues(_Output);
        }

        private static List<double[]> Check4NonValidValues(List<double[]> _input)
        {
            List<double[]> _output = new List<double[]>(_input);

            for (int i = 0; i < _input.Count; i++)
            {
                if (_input[i][1] == 0)
                {
                    _output[i - 2][1] = 0;
                    _output[i - 1][1] = 0;
                    _output[i + 1][1] = 0;
                    _output[i + 2][1] = 0;
                    // cutting away a bit more of the discontinuous part of the ele values
                    // (verified on checking some real data sets by accident)
                    break;
                }
            }
            return _output;
        } // if data set has invalide values (= points of discontinuity) -> values beside discontinuity will be also set tu invalide -> less artefacts at filtering method 

        public static double[] Calc_Elevation(List<double[]> data_list)
        {
            List<double[]> _data_list = SavitzkyGolayFilter(data_list);
            //List<double[]> _data_list = data_list; // without noise reduction
            double[] _elev = { 0, 0 };
            // first value = up, second value = down

            if (_data_list.Any())
            {
                double old_value = _data_list[0][0];

                for (int i = 1; i < _data_list.Count; i++)
                {

                    if (_data_list[i][1] == 1) // check for valid ele-value
                    {
                        double diff = _data_list[i][0] - old_value;
                        if (diff >= 0)
                        {
                            _elev[0] += diff; //up
                        }
                        else
                        {
                            _elev[1] += diff; // down
                        }
                    }

                    old_value = _data_list[i][0]; // this is the i-th value in the list and [0] is the ele-value
                }
            }
            return _elev;
        }

        private static bool Check4InvalideValues(int filter_width, List<double[]> _data_list, int position)
        {
            bool feedback = true;

            for (int i = position - filter_width; i < position + filter_width; i++)
            {
                if (_data_list[i][1] == 0) feedback = false;
            }
            return feedback;
        } // check elevation data for invalide values (= points of discontinuity) -> act as data crawler in SavitzkyGolayFilter method

        private static List<double[]> SavitzkyGolayFilter(List<double[]> data_list) 
        {
            List<double[]> Output = new List<double[]>(data_list); // initial Output list is equal to data_list (input list)

            for (int i = 0; i < data_list.Count; i++)
            {
                Output[i][1] = data_list[i][1]; // for the first two list items only valid-value is transfered (not necessary, but defined code)

                if ((i >= 12 & i < data_list.Count - 12) && Check4InvalideValues(12, data_list, i))
                {
                    Output[i][0] =
                    (data_list[i - 12][0] * -253 + data_list[i - 11][0] * -138
                    + data_list[i - 10][0] * -33 + data_list[i - 9][0] * 62
                    + data_list[i - 8][0] * 147 + data_list[i - 7][0] * 222
                    + data_list[i - 6][0] * 287 + data_list[i - 5][0] * 343
                    + data_list[i - 4][0] * 387 + data_list[i - 3][0] * 422
                    + data_list[i - 2][0] * 447 + data_list[i - 1][0] * 462
                    + data_list[i][0] * 467
                    + data_list[i + 1][0] * 462 + data_list[i + 2][0] * 447
                    + data_list[i + 3][0] * 422 + data_list[i + 4][0] * 387
                    + data_list[i + 5][0] * 343 + data_list[i + 6][0] * 287
                    + data_list[i + 7][0] * 222 + data_list[i + 8][0] * 147
                    + data_list[i + 9][0] * 62 + data_list[i + 10][0] * -33
                    + data_list[i + 11][0] * -138 + data_list[i + 12][0] * -253) / 5175;
                }
                else
                {
                    if ((i >= 8 & i < data_list.Count - 8) && Check4InvalideValues(8, data_list, i))
                    {
                        Output[i][0] = (data_list[i - 8][0] * -21
                            + data_list[i - 7][0] * -6
                            + data_list[i - 6][0] * 7
                            + data_list[i - 5][0] * 18
                            + data_list[i - 4][0] * 27
                            + data_list[i - 3][0] * 34
                            + data_list[i - 2][0] * 39
                            + data_list[i - 1][0] * 42
                            + data_list[i][0] * 43
                            + data_list[i + 1][0] * 42
                            + data_list[i + 2][0] * 39
                            + data_list[i + 3][0] * 34
                            + data_list[i + 4][0] * 27
                            + data_list[i + 5][0] * 18
                            + data_list[i + 6][0] * 7
                            + data_list[i + 7][0] * -6
                            + data_list[i + 8][0] * -21) / 323;
                    }
                    else
                    {
                        if ((i >= 4 & i < data_list.Count - 4) && Check4InvalideValues(4, data_list, i))
                        {
                            Output[i][0] =
                                (data_list[i - 4][0] * -21
                                + data_list[i - 3][0] * 14
                                + data_list[i - 2][0] * 39
                                + data_list[i - 1][0] * 54
                                + data_list[i][0] * 59
                                + data_list[i + 1][0] * 54
                                + data_list[i + 2][0] * 39
                                + data_list[i + 3][0] * 14
                                + data_list[i + 4][0] * -21) / 231;
                        }
                        else
                        {
                            if ((i >= 2 & i < data_list.Count - 2) && Check4InvalideValues(2, data_list, i))
                            {
                                Output[i][0] =
                                    (data_list[i - 2][0] * -3
                                    + data_list[i - 1][0] * 12
                                    + data_list[i][0] * 17
                                    + data_list[i + 1][0] * 12
                                    + data_list[i + 2][0] * -3) / 35;
                            }
                        }
                    }
                }
            }
            return Output;
        } // noise reduction method  http://www.statistics4u.info/fundstat_germ/cc_filter_savgolay.html
    }
}
