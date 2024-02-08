﻿using Equipment_Client.DB;
using Equipment_Client.Models;
using Equipment_Client.Tools;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Equipment_Client.VM.Responsible
{
    public class DiagrammVM : BaseVM
    {
        private Equipment selectEquipment;
        private int selectYear;

        public SeriesCollection SeriesViews { get; set; }
        public CustomCommand Sort { get; set; }
        public Func<double, string> YFormatter { get; set; }
        public string[] Labels { get; set; }
        public int MaxVal { get; set; } = 1;

        public List<Equipment> Equipments { get; set; }
        public Equipment SelectEquipment
        {
            get => selectEquipment;
            set
            {
                selectEquipment = value;
                Signal();
                Sort.Execute(null);
            }
        }
        public List<int> Years { get; set; }
        public int SelectYear 
        {
            get => selectYear;
            set
            {
                selectYear = value;
                Signal();
                Sort.Execute(null);
            }
        }


        public DiagrammVM(Scientist scientist)
        {
            try
            {
                Equipments = DBInstance.GetInstance().Equipment.Where(s => s.IdReponsibleScientists == scientist.Id).ToList();
                Years = new List<int>(new int[] {2022});
                Labels = new string[] { "январь", "февраль", "март", "апрель", "май", "июнь", "июль", "август", "сентябрь", "октябрь", "ноябрь", "декабрь" };
                YFormatter = value => value.ToString("0");
                int yearNow = DateTime.Now.Year;
                

                while (Years[Years.Count - 1] < yearNow) // Years[Years.Count - 1] - обращение к последнему элементу коллекции
                {
                    int lastYear = Years[Years.Count - 1];
                    Years.Add(lastYear + 1);
                }
                
                Sort = new CustomCommand(() =>
                {
                    

                    if (SelectEquipment != null)
                    {
                        var allBookings = DBInstance.GetInstance().Bookings
                            .Where(s => s.IdEquipmentNavigation.IdReponsibleScientists == scientist.Id &&
                                   s.IdEquipment == SelectEquipment.Id &&
                                   s.DateStart.Year == SelectYear)
                            .GroupBy(s => s.DateStart.Month);

                        

                        var bookingsApproved = DBInstance.GetInstance().Bookings
                            .Where(s => s.Approved == 1 && 
                                   s.IdEquipmentNavigation.IdReponsibleScientists == scientist.Id && 
                                   s.IdEquipment == SelectEquipment.Id &&
                                   s.DateStart.Year == SelectYear)
                            .GroupBy(s => s.DateStart.Month);
                        
                        int[] countsByMonthAllBookings = new int[12];
                        int[] countsByMonthBookingsApproved = new int[12];
                        for (int i = 0; i < 12; i++)
                        {
                            countsByMonthAllBookings[i] = allBookings.Where(s => s.Key == i + 1).Select(s => s.Count()).Sum();
                            countsByMonthBookingsApproved[i] = bookingsApproved.Where(s => s.Key == i + 1).Select(s => s.Count()).Sum();
                        }

                        int max1 = countsByMonthAllBookings.Max();
                        int max2 = countsByMonthBookingsApproved.Max();

                        if(max1 > max2)
                        {
                            MaxVal = max1 + 10;
                        }
                        else
                        {
                            MaxVal = max2 + 10;
                        }

                        SeriesViews = new SeriesCollection
                    {
                        new LineSeries
                        {
                            Title = "Все заявки",
                            Values = new ChartValues<int>(countsByMonthAllBookings),
                            PointGeometry = DefaultGeometries.Square,
                            PointGeometrySize = 15,
                            Fill = Brushes.Transparent
                        },

                    };

                        SeriesViews.Add(new LineSeries
                        {
                            Title = "Подтверждённые заявки",
                            Values = new ChartValues<int>(countsByMonthBookingsApproved),
                            PointGeometry = DefaultGeometries.Square,
                            PointGeometrySize = 15,
                            Fill = Brushes.Transparent
                        });
                        Signal(nameof(MaxVal));
                        Signal(nameof(SeriesViews));
                    }
                    
                });

                SelectYear = DateTime.Now.Year;
            }
            catch
            {
                MessageBox.Show("0");
                return;
            }

        }
    }
}
