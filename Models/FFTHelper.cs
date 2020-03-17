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

        public static double Hamming(double n, double frameSize)
        {
            return 0.54 - 0.46 * Math.Cos((2 * Math.PI * n) / (frameSize - 1));
        }

        public static double Hann(double n, double frameSize)
        {
            return 0.5 * (1 - Math.Cos((2 * Math.PI * n) / (frameSize - 1)));
        }

        public static double BlackmannHarris(double n, double frameSize)
        {
            return 0.35875 - (0.48829 * Math.Cos((2 * Math.PI * n) / (frameSize - 1))) +
                   (0.14128 * Math.Cos((4 * Math.PI * n) / (frameSize - 1))) - (0.01168 * Math.Cos((4 * Math.PI * n) / (frameSize - 1)));
        }


        /// <summary>
        /// Функция окна Хэннинга
        /// </summary>
        /// <param name="windowsize">длина фрейма для окна</param>
        /// <returns>массив со значениями оконной функции</returns>
        public static double[] hanning(int windowsize)
        {
            double[] w = new double[windowsize];
            for (int t = 0; t < windowsize; t++)
            {
                w[t] = 0.54 - 0.46 * Math.Cos(2 * Math.PI * t / windowsize);
            }
            return w;
        }


    }


}
