using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoiceСhanging.Models
{
    public class FFTHelper
    {
        public static string[] listWindow = new string[] {
            "Прямоугольное Окно","Окно Хамминга","Окно Ханна","Окно Блэкмэн-Харриса","Синус-Окно","Окно Барлетта","Окно Барлетта-Ханна","Окно Блэкмэна", "Окно Наталла"
        };


        private static Complex w(int k, int N)
        {
            if (k % N == 0) return 1;
            double arg = -2 * Math.PI * k / N;
            return new Complex(Math.Cos(arg), Math.Sin(arg));
        }
        public static Complex[] fft(Complex[] x)
        {
            Complex[] X;
            int N = x.Length; // длина массива отсчетов
          //  N -= (N % 2);

            if (N == 2)
            {
                X = new Complex[2];
                X[0] = x[0] + x[1];
                X[1] = x[0] - x[1];
            }
            else
            {
                Complex[] x_even = new Complex[N / 2]; // четные элементы
                Complex[] x_odd = new Complex[N / 2]; // нечетные элементы
                for (int i = 0; i < N / 2; i++)
                {
                    x_even[i] = x[2 * i];
                    x_odd[i] = x[2 * i + 1];
                }
                Complex[] X_even = fft(x_even);
                Complex[] X_odd = fft(x_odd);
                X = new Complex[N];
                for (int i = 0; i < N / 2; i++)
                {
                    X[i] = X_even[i] + w(i, N) * X_odd[i];
                X[i + N / 2] = X_even[i] - w(i, N) * X_odd[i];
                }
            }
            return X;
        }
        public static Complex[] nfft(Complex[] X)
        {
            int N = X.Length;
            //  N -= (N % 2);

            Complex[] X_n = new Complex[N];
            for (int i = 0; i < N / 2; i++)
            {
                X_n[i] = X[N / 2 + i];
                X_n[N / 2 + i] = X[i];
            }
            return X_n;
        }
  



        /// <summary>
        /// Функция быстрого преобразования Фурье для комплексного массива
        /// </summary>
        /// <param name="f">исходный комплексный массив</param>
        /// <returns>спектр массива</returns>
        public static Complex[] FFT(Complex[] f)
        {
            int N = f.Length;
            Complex[] F = new Complex[N];
            Complex[] o, O, e, E;
            if (N == 1)
            {
                F[0] = f[0];
                return F;
            }
            e = new Complex[N / 2];
            o = new Complex[N / 2];
            for (int i = 0; i < N / 2; i++)
            {
                e[i] = f[2 * i];
                o[i] = f[2 * i + 1];
            }
            O = FFT(o);
            E = FFT(e);
            for (int i = 0; i < N / 2; i++)
            {
                O[i] *= from_polar(1, -2 * Math.PI * i / N);
            }
            for (int i = 0; i < N / 2; i++)
            {
                F[i] = E[i] + O[i];
                F[i + N / 2] = E[i] - O[i];
            }
            return F;
        }

        /// <summary>
        /// Функция получения комплексного числа
        /// по полярным координатам
        /// </summary>
        /// <param name="r">модуль числа</param>
        /// <param name="arg">аргумент числа</param>
        /// <returns>комплексное число</returns>
        public static Complex from_polar(double r, double arg)
        {
            return new Complex(r * Math.Cos(arg), r * Math.Sin(arg));
        }


        /// <summary>
        /// Функция обратного преобразования Фурье с действительным результатом
        /// </summary>
        /// <param name="F">спектр Фурье</param>
        /// <returns>действительныймассив, восстановленный по спектру</returns>
        public static double[] IFFT(Complex[] F)
        {
            int N = F.Length;
            Complex[] f = IFFTc(F);
            double[] res = new double[N];
            for (int i = 0; i < N; i++)
            {
                res[i] = (f[i].Real + f[i].Imaginary) / N;
            }
            return res;
        }




        /// <summary>
        /// Функция обратного преобразования Фурье с комплексным результатом
        /// </summary>
        /// <param name="F">спектр Фурье</param>
        /// <returns>комплексный, восстановленный по спектру</returns>
        public static Complex[] IFFTc(Complex[] F)
        {
            int N = F.Length;
            Complex[] f = new Complex[N];
            Complex[] o, O, e, E;
            if (N == 1)
            {
                f[0] = F[0];
                return f;
                
            }
            e = new Complex[N / 2];
            o = new Complex[N / 2];
            for (int i = 0; i < N / 2; i++)
            {
                e[i] = conjugate(F[2 * i]);
                o[i] = conjugate(F[2 * i + 1]);
            }
            O = FFT(o);
            E = FFT(e);
            for (int i = 0; i < N / 2; i++)
            {
                O[i] *= from_polar(1, -2 * Math.PI * i / N);
            }
            for (int i = 0; i < N / 2; i++)
            {
                f[i] = conjugate(E[i] + O[i]);
                f[i + N / 2] = conjugate(E[i] - O[i]);
            }
            return f;
        }

        /// <summary>
        /// Функция комплексного сопряжения
        /// </summary>
        /// <param name="a">комплексное число</param>
        /// <returns>сопряжённое число</returns>
        public static Complex conjugate(Complex a)
        {
            return new Complex(a.Real, -a.Imaginary);
        }

       

        /// <summary>
        /// Функция окна Хэмминга
        /// </summary>
        /// <param name="windowsize">длина фрейма для окна</param>
        /// <returns>массив со значениями оконной функции</returns>
        public static double[] Hamming(int windowsize)
        {
            double[] w = new double[windowsize];
            for (int t = 0; t < windowsize; t++)
            {
                w[t] = 0.54f - 0.46f * Math.Cos(2 * Math.PI * t / (windowsize - 1));
            }
            return w;
        }


        public static double[] Hann(int windowsize)
        {
            double[] w = new double[windowsize];
            for (int n = 0; n < windowsize; n++)
            {
                w[n] = 0.5f - 0.5f * Math.Cos((2 * Math.PI * n) / (windowsize - 1));
            }
            return w;
        }

        public static double[] BlackmannHarris(int windowsize)
        {
            double[] w = new double[windowsize];
            for (int t = 0; t < windowsize; t++)
            {
                w[t] = 0.35875f - (0.48829f * Math.Cos((2 * Math.PI * t) / (windowsize - 1))) +
                   (0.14128f * Math.Cos((4 * Math.PI * t) / (windowsize - 1))) - (0.01168f * Math.Cos((4 * Math.PI * t) / (windowsize - 1)));

            }
            return w;
        }

        public static double[] RectangleWindow(int windowsize)
        {
            double[] w = new double[windowsize];
            for (int n = 0; n < windowsize; n++)
            {
                w[n] = 1;

            }

            return w;
        }


        public static double[] SinWindow(int windowsize)
        {
            double[] w = new double[windowsize];
            for (int n = 0; n < windowsize; n++)
            {
                w[n] = Math.Sin((Math.PI * n) / (windowsize - 1));
            }
            return w;
        }

        public static double[] BartlettWindow(int windowsize)
        {
            double[] w = new double[windowsize];
            for (int n = 0; n < windowsize; n++)
            {
                w[n] = 1 - ((n / ((windowsize - 1) / 2)) - 1);
            }
            return w;
        }

     
            public static double[] BartlettHannWindow(int windowsize)
        {
            double[] w = new double[windowsize];
            for (int n = 0; n < windowsize; n++)
            {
                w[n] = 0.62f - 0.48f * Math.Abs((n/ windowsize - 1) - 0.5f) - 0.38f * Math.Cos((2*Math.PI * n) /( windowsize - 1));
            }
            return w;
        }


        public static double[] Blackmann(int windowsize)
        {
            double[] w = new double[windowsize];
            for (int n = 0; n < windowsize; n++)
            {
                w[n] = 0.42f - 0.5f * Math.Cos((2 * Math.PI * n) / windowsize - 1) + 0.08f * Math.Cos((4 * Math.PI * n) / (windowsize - 1));

            }
            return w;
        }

        public static double[] NuttallWindow(int windowsize)
        {
            double[] w = new double[windowsize];
            for (int n = 0; n < windowsize; n++)
            {
                w[n] = 0.355768f - 0.487396f * Math.Cos((2 * Math.PI * n) / (windowsize - 1)) + 0.144232f * Math.Cos((4 * Math.PI * n) / (windowsize - 1)) - 0.012604f * Math.Cos((6 * Math.PI * n) / (windowsize - 1));
            }
            return w;
        }

        public static double[] BlackmanNuttallWindow(int windowsize)
        {
            double[] w = new double[windowsize];
            for (int n = 0; n < windowsize; n++)
            {
                w[n] = 0.3635819f - 0.4891775f * Math.Cos((2 * Math.PI * n) / (windowsize - 1)) + 0.1365995f * Math.Cos((4 * Math.PI * n) / (windowsize - 1)) - 0.0106411f * Math.Cos((6 * Math.PI * n) / (windowsize - 1));
            }
            return w;
        }



        public static double[] FlatTopWindow(int windowsize)
        {
            double[] w = new double[windowsize];
            for (int n = 0; n < windowsize; n++)
            {
                w[n] = 1.0f - 1.93f * Math.Cos((2 * Math.PI * n)/(windowsize - 1)) + 1.29f * Math.Cos((4 * Math.PI * n)/(windowsize - 1)) - 0.
            }
            return w;
        }





        public static double[] temp(int windowsize)
        {
            double[] w = new double[windowsize];

            return w;
        }



        public static double[] WindowFunc(string windowName, int windowSize)
        {
            switch (windowName)
            {
                case "Прямоугольное Окно":
                    return FFTHelper.RectangleWindow(windowSize);
                case "Окно Хамминга":
                    return FFTHelper.Hamming(windowSize);
                case "Окно Ханна":
                    return FFTHelper.Hann(windowSize);
                case "Окно Блэкмэн-Харриса":
                    return FFTHelper.BlackmannHarris(windowSize);
                case "Синус-Окно":
                    return FFTHelper.SinWindow(windowSize);
                case "Окно Барлетта":
                    return FFTHelper.BartlettWindow(windowSize);
                case "Окно Барлетта-Ханна":
                    return FFTHelper.BartlettHannWindow(windowSize);
                case "Окно Блэкмэна":
                    return FFTHelper.Blackmann(windowSize);
                case "Окно Наталла":
                    return FFTHelper.NuttallWindow(windowSize);

                default:
                    return FFTHelper.RectangleWindow(windowSize);

            }

        }



    }


}
