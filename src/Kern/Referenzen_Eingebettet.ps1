# Eingebettete Referenzprofile für Leos Minibench (v3.0)
$script:EmbeddedReferences = @{
    'Desktop_GTX1070.json' = @'
{
    "Format":  "PC-Diagnose-DB/2",
    "Name":  "Referenz: Desktop GTX 1070 (Ryzen 2700)",
    "Computer":  "Referenz-Desktop-GTX1070",
    "Geraet":  {
                   "Id":  "REF-REFERENZ-DESKTOP-GTX1070",
                   "Guete":  "hoch",
                   "Quellen":  [
                                   "Referenzsystem"
                               ]
               },
    "Datum":  "2026-10-11 17:21",
    "Version":  "3.61",
    "Quelle":  "Referenz",
    "Module":  "Benchmark",
    "Messdauer":  "normal",
    "System":  "Desktop Mittelklasse (AMD B450)",
    "Hardware":  {
                     "CPU":  "Ryzen 7 2700 Eight-Core (8 Kerne, 16 Threads)",
                     "Kerne":  8,
                     "Threads":  16,
                     "RAM":  "16 GB DDR4-2133",
                     "GPU":  "NVIDIA GeForce GTX 1070",
                     "IGPU":  null,
                     "GPUGemessen":  "NVIDIA GeForce GTX 1070",
                     "Datentraeger":  "NVMe PCIe 3.0 SSD 2TB; SATA SSD 250GB; HDD 2TB",
                     "Betriebssystem":  "Microsoft Windows 11",
                     "Mainboard":  "ASUS Desktop Mainboard",
                     "WindowsInstalliert":  null
                 },
    "Werte":  {
                  "CPU|ST":  2122,
                  "CPU|MT":  31744,
                  "CPU|AES":  944,
                  "CPU|SHA":  1693,
                  "CPU|DEFL":  234,
                  "RAM|Lesen":  29,
                  "RAM|Schreiben":  12.1,
                  "RAM|Kopieren":  8.8,
                  "RAM|Latenz":  109.5,
                  "GPU|REND":  74.4,
                  "GPU|REND1":  62.4,
                  "GPU|RPKT":  6857,
                  "GPU|VMB":  111.6,
                  "GPU|DWM":  6571,
                  "DISK|NVMe3|SR":  3438,
                  "DISK|NVMe3|SW":  2981,
                  "DISK|NVMe3|R1":  10166,
                  "DISK|NVMe3|R8":  72432,
                  "DISK|NVMe3|W1":  21082,
                  "DISK|SATA-SSD|SR":  552,
                  "DISK|SATA-SSD|SW":  527,
                  "DISK|SATA-SSD|R1":  8418,
                  "DISK|SATA-SSD|R8":  53525,
                  "DISK|SATA-SSD|W1":  13908,
                  "DISK|HDD|SR":  130,
                  "DISK|HDD|SW":  132,
                  "DISK|HDD|R1":  89,
                  "DISK|HDD|R8":  152,
                  "DISK|HDD|W1":  230
              },
    "Messwerte":  {
                      "CPU|AMD Ryzen 7 2700 Eight-Core Processor|ST":  2122,
                      "CPU|AMD Ryzen 7 2700 Eight-Core Processor|MT":  31744,
                      "CPU|AMD Ryzen 7 2700 Eight-Core Processor|AES":  944,
                      "CPU|AMD Ryzen 7 2700 Eight-Core Processor|SHA":  1693,
                      "CPU|AMD Ryzen 7 2700 Eight-Core Processor|DEFL":  234,
                      "RAM|16GB|2133|Lesen":  29,
                      "RAM|16GB|2133|Schreiben":  12.1,
                      "RAM|16GB|2133|Kopieren":  8.8,
                      "RAM|16GB|2133|Latenz":  109.5,
                      "GPU|NVIDIA GeForce GTX 1070|VMB":  111.6,
                      "GPU|NVIDIA GeForce GTX 1070|DWM":  6571,
                      "GPU|NVIDIA GeForce GTX 1070|REND|1280x720":  74.4,
                      "GPU|NVIDIA GeForce GTX 1070|REND1|1280x720":  62.4,
                      "GPU|NVIDIA GeForce GTX 1070|RPKT":  6857
                  },
    "Laufwerke":  [
                      {
                          "Laufwerk":  "KIOXIA-EXCERIA PLUS G3 SSD",
                          "Klasse":  "NVMe PCIe 3.0 x4",
                          "SR":  3438,
                          "SW":  2981,
                          "R1":  10166,
                          "R8":  72432,
                          "W1":  21082
                      },
                      {
                          "Laufwerk":  "Samsung SSD 850 EVO 250GB",
                          "Klasse":  "SATA-SSD",
                          "SR":  552,
                          "SW":  527,
                          "R1":  8418,
                          "R8":  53525,
                          "W1":  13908
                      },
                      {
                          "Laufwerk":  "ST2000DM006-2DM164",
                          "Klasse":  "Festplatte",
                          "SR":  127,
                          "SW":  134,
                          "R1":  75,
                          "R8":  130,
                          "W1":  226
                      }
                  ],
    "Befunde":  {
                    "Kritisch":  0,
                    "Warnungen":  0,
                    "Hinweise":  0,
                    "Liste":  [

                              ]
                },
    "Lasttest":  {
                     "CpuTempMax":  58,
                     "GpuTempMax":  65,
                     "CpuWMax":  43,
                     "GpuWMax":  165,
                     "Drosselung":  "keine",
                     "Taktstabilitaet":  99.3,
                     "TaktAbfall":  0.7
                 },
    "Rendertest":  [
                       {
                           "Grafik":  "NVIDIA GeForce GTX 1070",
                           "Art":  "dGPU",
                           "Aufloesung":  "1280x720",
                           "Fps":  74.4,
                           "Low1":  62.4,
                           "Punkte":  6857,
                           "Bildfehler":  0,
                           "Treiberreset":  0,
                           "Fehler":  ""
                       }
                   ]
}
'@
    'Desktop_HighEnd.json' = @'
{
    "Format":  "PC-Diagnose-DB/2",
    "Name":  "Referenz: Desktop High-End (Ryzen 9800X3D, RTX 5080)",
    "Computer":  "Referenz-Desktop-HighEnd",
    "Geraet":  {
                   "Id":  "REF-REFERENZ-DESKTOP-HIGHEND",
                   "Guete":  "hoch",
                   "Quellen":  [
                                   "Referenzsystem"
                               ]
               },
    "Datum":  "2026-10-17 12:00",
    "Version":  "3.85",
    "Quelle":  "Referenz",
    "Module":  "Benchmark",
    "Messdauer":  "normal",
    "System":  "Desktop High-End (AMD B650E)",
    "Hardware":  {
                     "CPU":  "Ryzen 7 9800X3D",
                     "Kerne":  8,
                     "Threads":  16,
                     "RAM":  "64 GB DDR5-6000",
                     "GPU":  "GeForce RTX 5080",
                     "IGPU":  null,
                     "GPUGemessen":  "NVIDIA GeForce RTX 5080",
                     "Datentraeger":  "Samsung SSD 990 PRO 4TB (3,64 TB, NVMe)",
                     "Betriebssystem":  "Microsoft Windows 11",
                     "Mainboard":  "ASUS ROG STRIX B650E-E GAMING WIFI",
                     "WindowsInstalliert":  null
                 },
    "Werte":  {
                  "GPU|DWM":  28722,
                  "RAM|Latenz":  80.7,
                  "GPU|VMB":  487.7,
                  "CPU|MT":  49750,
                  "DISK|NVMe4|W1":  59214,
                  "GPU|REND":  296.6,
                  "GPU|REND1":  73.3,
                  "DISK|NVMe4|R8":  177715,
                  "GPU|REND01":  73.1,
                  "RAM|Kopieren":  24.6,
                  "DISK|NVMe4|SR":  7443,
                  "RAM|Schreiben":  30.4,
                  "CPU|ST":  3282,
                  "CPU|AES":  1454,
                  "GPU|STUTTERREL":  14,
                  "CPU|SHA":  2561,
                  "GPU|RPKT":  27335,
                  "RAM|Lesen":  65.3,
                  "DISK|NVMe4|R1":  20590,
                  "CPU|DEFL":  380,
                  "DISK|NVMe4|SW":  6361
              },
    "Messwerte":  {
                      "CPU|AMD Ryzen 7 9800X3D 8-Core Processor|ST":  3282,
                      "CPU|AMD Ryzen 7 9800X3D 8-Core Processor|MT":  49750,
                      "CPU|AMD Ryzen 7 9800X3D 8-Core Processor|AES":  1454,
                      "CPU|AMD Ryzen 7 9800X3D 8-Core Processor|SHA":  2561,
                      "CPU|AMD Ryzen 7 9800X3D 8-Core Processor|DEFL":  380,
                      "RAM|64GB|6000|Lesen":  65.3,
                      "RAM|64GB|6000|Schreiben":  30.4,
                      "RAM|64GB|6000|Kopieren":  24.6,
                      "RAM|64GB|6000|Latenz":  80.7,
                      "GPU|NVIDIA GeForce RTX 5080|VMB":  487.7,
                      "GPU|NVIDIA GeForce RTX 5080|DWM":  28722,
                      "GPU|NVIDIA GeForce RTX 5080|REND|1280x720":  296.6,
                      "GPU|NVIDIA GeForce RTX 5080|REND1|1280x720":  73.3,
                      "GPU|NVIDIA GeForce RTX 5080|REND01|1280x720":  73.1,
                      "GPU|NVIDIA GeForce RTX 5080|STUTTER":  0,
                      "GPU|NVIDIA GeForce RTX 5080|STUTTERREL":  14.01,
                      "GPU|NVIDIA GeForce RTX 5080|RPKT":  27335,
                      "DISK|0025_3849_5140_69D7.|Samsung SSD 990 PRO 4TB|SR":  7443,
                      "DISK|0025_3849_5140_69D7.|Samsung SSD 990 PRO 4TB|SW":  6361,
                      "DISK|0025_3849_5140_69D7.|Samsung SSD 990 PRO 4TB|R1":  20590,
                      "DISK|0025_3849_5140_69D7.|Samsung SSD 990 PRO 4TB|R8":  177715,
                      "DISK|0025_3849_5140_69D7.|Samsung SSD 990 PRO 4TB|W1":  59214
                  },
    "Ordner":  "",
    "Laufwerke":  [
                      {
                          "Laufwerk":  "Samsung SSD 990 PRO 4TB (C:)",
                          "Klasse":  "NVMe PCIe 4.0 x4",
                          "SR":  7443,
                          "SW":  6361,
                          "R1":  20590,
                          "R8":  177715,
                          "W1":  59214
                      }
                  ],
    "Befunde":  {
                    "Kritisch":  0,
                    "Warnungen":  0,
                    "Hinweise":  0,
                    "Liste":  [

                              ]
                },
    "Lasttest":  {
                     "CpuTempMax":  58,
                     "GpuTempMax":  54,
                     "CpuWMax":  87,
                     "GpuWMax":  285,
                     "Drosselung":  "keine",
                     "Taktstabilitaet":  99,
                     "TaktAbfall":  1
                 },
    "Rendertest":  [
                       {
                           "Grafik":  "NVIDIA GeForce RTX 5080",
                           "Art":  "dGPU",
                           "Aufloesung":  "1280x720",
                           "Fps":  296.6,
                           "Low1":  73.3,
                           "Punkte":  27335,
                           "Bildfehler":  0,
                           "Treiberreset":  false,
                           "Fehler":  ""
                       }
                   ],
    "Schreibzugriffe":  null,
    "Ablauf":  "normal",
    "Sensoren":  {
                     "Benchmark":  {
                                       "CpuTempLeerlauf":  38.5,
                                       "CpuTempMax":  48.1,
                                       "TjMax":  null,
                                       "CpuWMax":  86.5,
                                       "CpuMHzMax":  5225,
                                       "Gpu":  "NVIDIA GeForce RTX 5080",
                                       "GpuTempMax":  53.7,
                                       "GpuWMax":  284.4,
                                       "IGpu":  "",
                                       "IGpuTempMax":  null,
                                       "IGpuWMax":  null,
                                       "LuefterMax":  1720,
                                       "DatentraegerTempMax":  40.9,
                                       "Messpunkte":  46,
                                       "Quelle":  "Quellen: LibreHardwareMonitor 0.9.6, nvidia-smi, Windows-Leistungszähler, ACPI; Treiber: PawnIO 2.2.0.0 (bereits vorhanden)",
                                       "Abschnitte":  [
                                                          {
                                                              "Teil":  "Prozessor",
                                                              "CpuTempMax":  48.1,
                                                              "CpuWMax":  86.5,
                                                              "CpuMHzAvg":  4838,
                                                              "GpuTempMax":  26.8,
                                                              "GpuWMax":  36.2
                                                          },
                                                          {
                                                              "Teil":  "Arbeitsspeicher",
                                                              "CpuTempMax":  47.4,
                                                              "CpuWMax":  75,
                                                              "CpuMHzAvg":  5054,
                                                              "GpuTempMax":  25.7,
                                                              "GpuWMax":  21.5
                                                          },
                                                          {
                                                              "Teil":  "Grafik",
                                                              "CpuTempMax":  47.4,
                                                              "CpuWMax":  64.6,
                                                              "CpuMHzAvg":  4883,
                                                              "GpuTempMax":  53.7,
                                                              "GpuWMax":  284.4
                                                          },
                                                          {
                                                              "Teil":  "Datenträger",
                                                              "CpuTempMax":  42.4,
                                                              "CpuWMax":  53.5,
                                                              "CpuMHzAvg":  4754,
                                                              "GpuTempMax":  35.5,
                                                              "GpuWMax":  54.4
                                                          }
                                                      ]
                                   },
                     "Verbrauch":  {
                                       "IsNotebook":  false,
                                       "Leerlauf":  {
                                                        "Verfuegbar":  true,
                                                        "Text":  "~56 W",
                                                        "P_AC":  56,
                                                        "P_DC":  45,
                                                        "CpuW":  15.6,
                                                        "GpuW":  12,
                                                        "RamW":  3,
                                                        "BoardW":  10,
                                                        "DiskW":  1.8,
                                                        "FanW":  1.8,
                                                        "VrmW":  0.8,
                                                        "Wirkungsgrad":  0.8054,
                                                        "WirkungsgradProzent":  80.5,
                                                        "LoadPercent":  6.9,
                                                        "PsuWattage":  650,
                                                        "PsuRating":  "80 PLUS Bronze",
                                                        "IsNotebook":  false,
                                                        "Aufschluesselung":  "Gemessen: CPU 16 W, GPU 12 W + Wandler/VRM 0,8 W, Mainboard 10 W, RAM 3,0 W (2 DIMMs), Datenträger 1,8 W, Lüfter 1,8 W (3 Lüfter) = intern ~45 W | ~56 W an der Steckdose (intern ~45 W | PSU-Last ~7 % @ ~80,5 % Effizienz [650 W, 80 PLUS Bronze])",
                                                        "AufschluesselungKurz":  "~56 W an der Steckdose (intern ~45 W | PSU-Last ~7 % @ ~80,5 % Effizienz [650 W, 80 PLUS Bronze])"
                                                    },
                                       "Teillast":  {
                                                        "Verfuegbar":  true,
                                                        "Text":  "~106 W",
                                                        "P_AC":  106,
                                                        "P_DC":  89.2,
                                                        "CpuW":  35.6,
                                                        "GpuW":  27,
                                                        "RamW":  5,
                                                        "BoardW":  12,
                                                        "DiskW":  3,
                                                        "FanW":  3,
                                                        "VrmW":  3.6,
                                                        "Wirkungsgrad":  0.843,
                                                        "WirkungsgradProzent":  84.3,
                                                        "LoadPercent":  13.7,
                                                        "PsuWattage":  650,
                                                        "PsuRating":  "80 PLUS Bronze",
                                                        "IsNotebook":  false,
                                                        "Aufschluesselung":  "Gemessen: CPU 36 W, GPU 27 W + Wandler/VRM 3,6 W, Mainboard 12 W, RAM 5,0 W (2 DIMMs), Datenträger 3,0 W, Lüfter 3,0 W (3 Lüfter) = intern ~89 W | ~106 W an der Steckdose (intern ~89 W | PSU-Last ~14 % @ ~84,3 % Effizienz [650 W, 80 PLUS Bronze])",
                                                        "AufschluesselungKurz":  "~106 W an der Steckdose (intern ~89 W | PSU-Last ~14 % @ ~84,3 % Effizienz [650 W, 80 PLUS Bronze])"
                                                    },
                                       "Gaming":  {
                                                      "Verfuegbar":  true,
                                                      "Text":  "~438 W",
                                                      "P_AC":  438,
                                                      "P_DC":  384.8,
                                                      "CpuW":  64.9,
                                                      "GpuW":  284.4,
                                                      "RamW":  8,
                                                      "BoardW":  14,
                                                      "DiskW":  2.5,
                                                      "FanW":  4.5,
                                                      "VrmW":  6.5,
                                                      "Wirkungsgrad":  0.8795,
                                                      "WirkungsgradProzent":  87.9,
                                                      "LoadPercent":  59.2,
                                                      "PsuWattage":  650,
                                                      "PsuRating":  "80 PLUS Bronze",
                                                      "IsNotebook":  false,
                                                      "Aufschluesselung":  "Gemessen: CPU 65 W, GPU 284 W + Wandler/VRM 6,5 W, Mainboard 14 W, RAM 8,0 W (2 DIMMs), Datenträger 2,5 W, Lüfter 4,5 W (3 Lüfter) = intern ~385 W | ~438 W an der Steckdose (intern ~385 W | PSU-Last ~59 % @ ~87,9 % Effizienz [650 W, 80 PLUS Bronze])",
                                                      "AufschluesselungKurz":  "~438 W an der Steckdose (intern ~385 W | PSU-Last ~59 % @ ~87,9 % Effizienz [650 W, 80 PLUS Bronze])"
                                                  },
                                       "Volllast":  {
                                                        "Verfuegbar":  true,
                                                        "Text":  "~469 W",
                                                        "P_AC":  469,
                                                        "P_DC":  411.5,
                                                        "CpuW":  86.5,
                                                        "GpuW":  284.4,
                                                        "RamW":  8,
                                                        "BoardW":  14,
                                                        "DiskW":  5.5,
                                                        "FanW":  4.5,
                                                        "VrmW":  8.6,
                                                        "Wirkungsgrad":  0.877,
                                                        "WirkungsgradProzent":  87.7,
                                                        "LoadPercent":  63.3,
                                                        "PsuWattage":  650,
                                                        "PsuRating":  "80 PLUS Bronze",
                                                        "IsNotebook":  false,
                                                        "Aufschluesselung":  "Gemessen: CPU 87 W, GPU 284 W + Wandler/VRM 8,6 W, Mainboard 14 W, RAM 8,0 W (2 DIMMs), Datenträger 5,5 W, Lüfter 4,5 W (3 Lüfter) = intern ~412 W | ~469 W an der Steckdose (intern ~412 W | PSU-Last ~63 % @ ~87,7 % Effizienz [650 W, 80 PLUS Bronze])",
                                                        "AufschluesselungKurz":  "~469 W an der Steckdose (intern ~412 W | PSU-Last ~63 % @ ~87,7 % Effizienz [650 W, 80 PLUS Bronze])"
                                                    },
                                       "Effizienz":  {
                                                         "CpuMT_PerWatt":  575.1,
                                                         "Gpu_PerWatt":  1.04,
                                                         "System_PerWatt":  41.6
                                                     },
                                       "PsuWattage":  650,
                                       "PsuRating":  "80 PLUS Bronze"
                                   }
                 },
    "Optimierung":  null,
    "Akku":  null
}
'@
    'Desktop_HighEnd_RX6800.json' = @'
{
    "Format":  "PC-Diagnose-DB/2",
    "Name":  "Referenz: Desktop High-End (Ryzen 7600X, RX 6800)",
    "Computer":  "Referenz-Desktop-HighEnd-RX6800",
    "Geraet":  {
                   "Id":  "REF-REFERENZ-DESKTOP-HIGHEND-RX6800",
                   "Guete":  "hoch",
                   "Quellen":  [
                                   "Referenzsystem"
                               ]
               },
    "Datum":  "2026-10-01 12:00",
    "Version":  "2.95",
    "Quelle":  "Referenz",
    "Module":  "Benchmark",
    "Messdauer":  "normal",
    "System":  "Desktop High-End (AMD B650)",
    "Hardware":  {
                     "CPU":  "AMD Ryzen 5 7600X",
                     "Kerne":  6,
                     "Threads":  12,
                     "RAM":  "32 GB DDR5-6000",
                     "GPU":  "Radeon RX 6800",
                     "IGPU":  null,
                     "GPUGemessen":  "AMD Radeon RX 6800",
                     "Datentraeger":  "NVMe PCIe 4.0 SSD 1TB; NVMe PCIe 3.0 SSD 1TB; SATA SSD 1TB",
                     "Betriebssystem":  "Microsoft Windows 11",
                     "Mainboard":  "AMD B650 Mainboard",
                     "WindowsInstalliert":  null
                 },
    "Werte":  {
                  "DISK|NVMe4|SW":  2779,
                  "RAM|Lesen":  61.9,
                  "CPU|MT":  38883,
                  "DISK|NVMe4|R8":  119846,
                  "DISK|SATA-SSD|SW":  524,
                  "DISK|NVMe4|SR":  6551,
                  "RAM|Kopieren":  24.1,
                  "GPU|REND":  133.3,
                  "GPU|REND1":  121,
                  "DISK|SATA-SSD|R8":  73138,
                  "DISK|SATA-SSD|R1":  11337,
                  "DISK|NVMe3|R8":  109080,
                  "DISK|SATA-SSD|SR":  558,
                  "CPU|AES":  1429,
                  "DISK|NVMe3|W1":  58294,
                  "GPU|RPKT":  12285,
                  "GPU|VMB":  296.1,
                  "GPU|DWM":  17440,
                  "DISK|NVMe3|R1":  14090,
                  "DISK|NVMe3|SW":  2910,
                  "CPU|ST":  3092,
                  "DISK|NVMe4|R1":  14178,
                  "DISK|SATA-SSD|W1":  29740,
                  "DISK|NVMe3|SR":  3114,
                  "RAM|Latenz":  80.6,
                  "RAM|Schreiben":  31.3,
                  "DISK|NVMe4|W1":  52665,
                  "CPU|SHA":  2535,
                  "CPU|DEFL":  296
              },
    "Messwerte":  {
                      "CPU|AMD Ryzen 5 7600X 6-Core Processor|ST":  3092,
                      "CPU|AMD Ryzen 5 7600X 6-Core Processor|MT":  38883,
                      "CPU|AMD Ryzen 5 7600X 6-Core Processor|AES":  1429,
                      "CPU|AMD Ryzen 5 7600X 6-Core Processor|SHA":  2535,
                      "CPU|AMD Ryzen 5 7600X 6-Core Processor|DEFL":  296,
                      "RAM|32GB|6000|Lesen":  61.9,
                      "RAM|32GB|6000|Schreiben":  31.3,
                      "RAM|32GB|6000|Kopieren":  24.1,
                      "RAM|32GB|6000|Latenz":  80.6,
                      "GPU|AMD Radeon RX 6800|VMB":  296.1,
                      "GPU|AMD Radeon RX 6800|DWM":  17440,
                      "GPU|AMD Radeon RX 6800|REND|1280x720":  133.3,
                      "GPU|AMD Radeon RX 6800|REND1|1280x720":  121,
                      "GPU|AMD Radeon RX 6800|RPKT":  12285,
                      "DISK|203791800341|SanDisk SDSSDH3 1T02|SR":  554,
                      "DISK|203791800341|SanDisk SDSSDH3 1T02|SW":  514,
                      "DISK|203791800341|SanDisk SDSSDH3 1T02|R1":  10433,
                      "DISK|203791800341|SanDisk SDSSDH3 1T02|R8":  70639,
                      "DISK|203791800341|SanDisk SDSSDH3 1T02|W1":  29362,
                      "DISK|S4X6NF0MC07136F|Samsung SSD 860 EVO 1TB|SR":  563,
                      "DISK|S4X6NF0MC07136F|Samsung SSD 860 EVO 1TB|SW":  533,
                      "DISK|S4X6NF0MC07136F|Samsung SSD 860 EVO 1TB|R1":  12241,
                      "DISK|S4X6NF0MC07136F|Samsung SSD 860 EVO 1TB|R8":  75637,
                      "DISK|S4X6NF0MC07136F|Samsung SSD 860 EVO 1TB|W1":  30117,
                      "DISK|0025_38B6_31A3_2B45.|SAMSUNG MZVL21T0HCLR-00B00|SR":  6551,
                      "DISK|0025_38B6_31A3_2B45.|SAMSUNG MZVL21T0HCLR-00B00|SW":  2779,
                      "DISK|0025_38B6_31A3_2B45.|SAMSUNG MZVL21T0HCLR-00B00|R1":  14178,
                      "DISK|0025_38B6_31A3_2B45.|SAMSUNG MZVL21T0HCLR-00B00|R8":  119846,
                      "DISK|0025_38B6_31A3_2B45.|SAMSUNG MZVL21T0HCLR-00B00|W1":  52665,
                      "DISK|0000_0006_2303_2570_CAF2_5B03_FF00_024E.|Lexar SSD NM620 1TB|SR":  2805,
                      "DISK|0000_0006_2303_2570_CAF2_5B03_FF00_024E.|Lexar SSD NM620 1TB|SW":  2908,
                      "DISK|0000_0006_2303_2570_CAF2_5B03_FF00_024E.|Lexar SSD NM620 1TB|R1":  16504,
                      "DISK|0000_0006_2303_2570_CAF2_5B03_FF00_024E.|Lexar SSD NM620 1TB|R8":  124476,
                      "DISK|0000_0006_2303_2570_CAF2_5B03_FF00_024E.|Lexar SSD NM620 1TB|W1":  48673,
                      "DISK|E823_8FA6_BF53_0001_001B_444A_463E_D685.|WDS100T3X0C-00SJG0|SR":  3422,
                      "DISK|E823_8FA6_BF53_0001_001B_444A_463E_D685.|WDS100T3X0C-00SJG0|SW":  2912,
                      "DISK|E823_8FA6_BF53_0001_001B_444A_463E_D685.|WDS100T3X0C-00SJG0|R1":  11676,
                      "DISK|E823_8FA6_BF53_0001_001B_444A_463E_D685.|WDS100T3X0C-00SJG0|R8":  93685,
                      "DISK|E823_8FA6_BF53_0001_001B_444A_463E_D685.|WDS100T3X0C-00SJG0|W1":  67914,
                      "WINSAT|CPU":  9.3,
                      "WINSAT|RAM":  9.3,
                      "WINSAT|DISK":  9.35,
                      "WINSAT|GFX":  9.9
                  },
    "Ordner":  "",
    "Laufwerke":  [
                      {
                          "Laufwerk":  "SanDisk SDSSDH3 1T02",
                          "Klasse":  "SATA-SSD",
                          "SR":  554,
                          "SW":  514,
                          "R1":  10433,
                          "R8":  70639,
                          "W1":  29362
                      },
                      {
                          "Laufwerk":  "Samsung SSD 860 EVO 1TB",
                          "Klasse":  "SATA-SSD",
                          "SR":  563,
                          "SW":  533,
                          "R1":  12241,
                          "R8":  75637,
                          "W1":  30117
                      },
                      {
                          "Laufwerk":  "SAMSUNG MZVL21T0HCLR-00B00",
                          "Klasse":  "NVMe PCIe 4.0 x4",
                          "SR":  6551,
                          "SW":  2779,
                          "R1":  14178,
                          "R8":  119846,
                          "W1":  52665
                      },
                      {
                          "Laufwerk":  "Lexar SSD NM620 1TB",
                          "Klasse":  "NVMe PCIe 3.0 x4",
                          "SR":  2805,
                          "SW":  2908,
                          "R1":  16504,
                          "R8":  124476,
                          "W1":  48673
                      },
                      {
                          "Laufwerk":  "WDS100T3X0C-00SJG0",
                          "Klasse":  "NVMe PCIe 3.0 x4",
                          "SR":  3422,
                          "SW":  2912,
                          "R1":  11676,
                          "R8":  93685,
                          "W1":  67914
                      }
                  ],
    "Befunde":  {
                    "Kritisch":  0,
                    "Warnungen":  0,
                    "Hinweise":  0,
                    "Liste":  [

                              ]
                },
    "Lasttest":  {
                     "CpuTempMax":  75,
                     "GpuTempMax":  68,
                     "CpuWMax":  85,
                     "GpuWMax":  215,
                     "Drosselung":  "keine",
                     "Taktstabilitaet":  98.0,
                     "TaktAbfall":  2.0
                 },
    "Rendertest":  [
                       {
                           "Grafik":  "AMD Radeon RX 6800",
                           "Art":  "dGPU",
                           "Aufloesung":  "1280x720",
                           "Fps":  133.3,
                           "Low1":  121,
                           "Punkte":  12285,
                           "Bildfehler":  0,
                           "Treiberreset":  false,
                           "Fehler":  ""
                       }
                   ],
    "Schreibzugriffe":  null,
    "Ablauf":  "normal",
    "Sensoren":  {

                 },
    "Optimierung":  null,
    "Akku":  null
}
'@
    'Desktop_Mittelklasse.json' = @'
{
    "Format":  "PC-Diagnose-DB/2",
    "Name":  "Referenz: Desktop Mittelklasse (Ryzen 5600, RX 570)",
    "Computer":  "Referenz-Desktop-Mittelklasse",
    "Geraet":  {
                   "Id":  "REF-REFERENZ-DESKTOP-MITTELKLASSE",
                   "Guete":  "hoch",
                   "Quellen":  [
                                   "Referenzsystem"
                               ]
               },
    "Datum":  "2026-10-01 12:00",
    "Version":  "2.95",
    "Quelle":  "Referenz",
    "Module":  "Benchmark",
    "Messdauer":  "normal",
    "System":  "Desktop Mittelklasse (AMD B450)",
    "Hardware":  {
                     "CPU":  "AMD Ryzen 5 5600",
                     "Kerne":  6,
                     "Threads":  12,
                     "RAM":  "32 GB DDR4-2133",
                     "GPU":  "Radeon RX 570 Series",
                     "IGPU":  null,
                     "GPUGemessen":  "Radeon RX 570 Series",
                     "Datentraeger":  "NVMe PCIe 3.0 SSD 500GB; SATA SSD 1TB",
                     "Betriebssystem":  "Microsoft Windows 11",
                     "Mainboard":  "AMD B450 Mainboard",
                     "WindowsInstalliert":  null
                 },
    "Werte":  {
                  "DISK|NVMe3|R8":  108045,
                  "GPU|DWM":  4339,
                  "RAM|Latenz":  113.2,
                  "GPU|VMB":  73.7,
                  "CPU|MT":  30319,
                  "DISK|HDD|SW":  123,
                  "GPU|REND":  45.4,
                  "GPU|REND1":  42.8,
                  "DISK|HDD|W1":  1412,
                  "DISK|HDD|R8":  178,
                  "RAM|Kopieren":  11.5,
                  "RAM|Schreiben":  13.4,
                  "CPU|ST":  2731,
                  "CPU|AES":  1160,
                  "DISK|HDD|SR":  112,
                  "DISK|NVMe3|W1":  35518,
                  "CPU|SHA":  2192,
                  "GPU|RPKT":  4189,
                  "RAM|Lesen":  27.5,
                  "DISK|NVMe3|SW":  2591,
                  "DISK|NVMe3|R1":  14329,
                  "DISK|NVMe3|SR":  3095,
                  "CPU|DEFL":  244,
                  "DISK|HDD|R1":  104
              },
    "Messwerte":  {
                      "CPU|AMD Ryzen 5 5600 6-Core Processor|ST":  2731,
                      "CPU|AMD Ryzen 5 5600 6-Core Processor|MT":  30319,
                      "CPU|AMD Ryzen 5 5600 6-Core Processor|AES":  1160,
                      "CPU|AMD Ryzen 5 5600 6-Core Processor|SHA":  2192,
                      "CPU|AMD Ryzen 5 5600 6-Core Processor|DEFL":  244,
                      "RAM|32GB|2133|Lesen":  27.5,
                      "RAM|32GB|2133|Schreiben":  13.4,
                      "RAM|32GB|2133|Kopieren":  11.5,
                      "RAM|32GB|2133|Latenz":  113.2,
                      "GPU|Radeon RX 570 Series|VMB":  73.7,
                      "GPU|Radeon RX 570 Series|DWM":  4339,
                      "GPU|Radeon RX 570 Series|REND|1280x720":  45.4,
                      "GPU|Radeon RX 570 Series|REND1|1280x720":  42.8,
                      "GPU|Radeon RX 570 Series|RPKT":  4189,
                      "DISK|Z1E7D75M|ST2000DX001-1CM164|SR":  110,
                      "DISK|Z1E7D75M|ST2000DX001-1CM164|SW":  128,
                      "DISK|Z1E7D75M|ST2000DX001-1CM164|R1":  106,
                      "DISK|Z1E7D75M|ST2000DX001-1CM164|R8":  172,
                      "DISK|Z1E7D75M|ST2000DX001-1CM164|W1":  1339,
                      "DISK|ZFN03R4F|ST4000DM004-2CV104|SR":  114,
                      "DISK|ZFN03R4F|ST4000DM004-2CV104|SW":  118,
                      "DISK|ZFN03R4F|ST4000DM004-2CV104|R1":  103,
                      "DISK|ZFN03R4F|ST4000DM004-2CV104|R8":  185,
                      "DISK|ZFN03R4F|ST4000DM004-2CV104|W1":  1485,
                      "DISK|0025_38D9_31A0_34FA.|Samsung SSD 980 1TB|SR":  3095,
                      "DISK|0025_38D9_31A0_34FA.|Samsung SSD 980 1TB|SW":  2591,
                      "DISK|0025_38D9_31A0_34FA.|Samsung SSD 980 1TB|R1":  14329,
                      "DISK|0025_38D9_31A0_34FA.|Samsung SSD 980 1TB|R8":  108045,
                      "DISK|0025_38D9_31A0_34FA.|Samsung SSD 980 1TB|W1":  35518,
                      "WINSAT|CPU":  9.3,
                      "WINSAT|RAM":  9.3,
                      "WINSAT|DISK":  8.8,
                      "WINSAT|GFX":  8.7
                  },
    "Ordner":  "",
    "Laufwerke":  [
                      {
                          "Laufwerk":  "ST2000DX001-1CM164",
                          "Klasse":  "Festplatte",
                          "SR":  110,
                          "SW":  128,
                          "R1":  106,
                          "R8":  172,
                          "W1":  1339
                      },
                      {
                          "Laufwerk":  "ST4000DM004-2CV104",
                          "Klasse":  "Festplatte",
                          "SR":  114,
                          "SW":  118,
                          "R1":  103,
                          "R8":  185,
                          "W1":  1485
                      },
                      {
                          "Laufwerk":  "Samsung SSD 980 1TB",
                          "Klasse":  "NVMe PCIe 3.0 x4",
                          "SR":  3095,
                          "SW":  2591,
                          "R1":  14329,
                          "R8":  108045,
                          "W1":  35518
                      }
                  ],
    "Befunde":  {
                    "Kritisch":  0,
                    "Warnungen":  0,
                    "Hinweise":  0,
                    "Liste":  [

                              ]
                },
    "Lasttest":  {
                     "CpuTempMax":  72,
                     "GpuTempMax":  74,
                     "CpuWMax":  76,
                     "GpuWMax":  135,
                     "Drosselung":  "keine",
                     "Taktstabilitaet":  97.0,
                     "TaktAbfall":  3.0
                 },
    "Rendertest":  [
                       {
                           "Grafik":  "Radeon RX 570 Series",
                           "Art":  "dGPU",
                           "Aufloesung":  "1280x720",
                           "Fps":  45.4,
                           "Low1":  42.8,
                           "Punkte":  4189,
                           "Bildfehler":  0,
                           "Treiberreset":  false,
                           "Fehler":  ""
                       }
                   ],
    "Schreibzugriffe":  null,
    "Ablauf":  "normal",
    "Sensoren":  {

                 },
    "Optimierung":  null,
    "Akku":  null
}
'@
    'MiniPC_APU.json' = @'
{
    "Format":  "PC-Diagnose-DB/2",
    "Name":  "Referenz: Mini-PC / APU (Ryzen 7 255H, Radeon 780M)",
    "Computer":  "Referenz-MiniPC-APU",
    "Geraet":  {
                   "Id":  "REF-REFERENZ-MINIPC-APU",
                   "Guete":  "hoch",
                   "Quellen":  [
                                   "Referenzsystem"
                               ]
               },
    "Datum":  "2026-10-01 12:00",
    "Version":  "2.95",
    "Quelle":  "Referenz",
    "Module":  "Benchmark",
    "Messdauer":  "normal",
    "System":  "Mini-PC / APU (AMD Ryzen 7)",
    "Hardware":  {
                     "CPU":  "AMD Ryzen 7 H 255 w/ Radeon 780M Graphics",
                     "Kerne":  8,
                     "Threads":  16,
                     "RAM":  "32 GB DDR5-5600",
                     "GPU":  "Radeon 780M Graphics",
                     "IGPU":  "Radeon 780M Graphics",
                     "GPUGemessen":  "AMD Radeon 780M Graphics",
                     "Datentraeger":  "NVMe PCIe 4.0 SSD 1TB",
                     "Betriebssystem":  "Microsoft Windows 11",
                     "Mainboard":  "Mini-PC Mainboard",
                     "WindowsInstalliert":  null
                 },
    "Werte":  {
                  "GPU|DWM":  1670,
                  "RAM|Latenz":  112,
                  "GPU|VMB":  28.4,
                  "CPU|MT":  46553,
                  "DISK|NVMe4|W1":  54294,
                  "GPU|REND":  46.8,
                  "GPU|REND1":  42.3,
                  "DISK|NVMe4|R8":  34279,
                  "RAM|Kopieren":  17.4,
                  "DISK|NVMe4|SR":  4295,
                  "RAM|Schreiben":  25.4,
                  "CPU|ST":  2928,
                  "CPU|AES":  1362,
                  "CPU|SHA":  2409,
                  "GPU|RPKT":  4313,
                  "RAM|Lesen":  58.2,
                  "DISK|NVMe4|R1":  4519,
                  "CPU|DEFL":  345,
                  "DISK|NVMe4|SW":  1140
              },
    "Messwerte":  {
                      "CPU|AMD Ryzen 7 H 255 w/ Radeon 780M Graphics|ST":  2928,
                      "CPU|AMD Ryzen 7 H 255 w/ Radeon 780M Graphics|MT":  46553,
                      "CPU|AMD Ryzen 7 H 255 w/ Radeon 780M Graphics|AES":  1362,
                      "CPU|AMD Ryzen 7 H 255 w/ Radeon 780M Graphics|SHA":  2409,
                      "CPU|AMD Ryzen 7 H 255 w/ Radeon 780M Graphics|DEFL":  345,
                      "RAM|32GB|5600|Lesen":  58.2,
                      "RAM|32GB|5600|Schreiben":  25.4,
                      "RAM|32GB|5600|Kopieren":  17.4,
                      "RAM|32GB|5600|Latenz":  112,
                      "GPU|AMD Radeon 780M Graphics|VMB":  28.4,
                      "GPU|AMD Radeon 780M Graphics|DWM":  1670,
                      "GPU|AMD Radeon 780M Graphics|REND|1280x720":  46.8,
                      "GPU|AMD Radeon 780M Graphics|REND1|1280x720":  42.3,
                      "GPU|AMD Radeon 780M Graphics|RPKT":  4313,
                      "DISK|ACE4_2E00_3A0E_DF9B_2EE4_AC00_0000_0001.|PC801 NVMe SK hynix 1TB|SR":  4295,
                      "DISK|ACE4_2E00_3A0E_DF9B_2EE4_AC00_0000_0001.|PC801 NVMe SK hynix 1TB|SW":  1140,
                      "DISK|ACE4_2E00_3A0E_DF9B_2EE4_AC00_0000_0001.|PC801 NVMe SK hynix 1TB|R1":  4519,
                      "DISK|ACE4_2E00_3A0E_DF9B_2EE4_AC00_0000_0001.|PC801 NVMe SK hynix 1TB|R8":  34279,
                      "DISK|ACE4_2E00_3A0E_DF9B_2EE4_AC00_0000_0001.|PC801 NVMe SK hynix 1TB|W1":  54294,
                      "WINSAT|CPU":  9.4,
                      "WINSAT|RAM":  9.4,
                      "WINSAT|DISK":  8.45,
                      "WINSAT|GFX":  8.2
                  },
    "Ordner":  "",
    "Laufwerke":  [
                      {
                          "Laufwerk":  "PC801 NVMe SK hynix 1TB",
                          "Klasse":  "NVMe PCIe 4.0 x4",
                          "SR":  4295,
                          "SW":  1140,
                          "R1":  4519,
                          "R8":  34279,
                          "W1":  54294
                      }
                  ],
    "Befunde":  {
                    "Kritisch":  0,
                    "Warnungen":  0,
                    "Hinweise":  0,
                    "Liste":  [

                              ]
                },
    "Lasttest":  {
                     "CpuTempMax":  78,
                     "GpuTempMax":  72,
                     "CpuWMax":  65,
                     "GpuWMax":  30,
                     "Drosselung":  "keine",
                     "Taktstabilitaet":  96.0,
                     "TaktAbfall":  4.0
                 },
    "Rendertest":  [
                       {
                           "Grafik":  "AMD Radeon 780M Graphics",
                           "Art":  "iGPU",
                           "Aufloesung":  "1280x720",
                           "Fps":  46.8,
                           "Low1":  42.3,
                           "Punkte":  4313,
                           "Bildfehler":  0,
                           "Treiberreset":  false,
                           "Fehler":  ""
                       }
                   ],
    "Schreibzugriffe":  null,
    "Ablauf":  "normal",
    "Sensoren":  {

                 },
    "Optimierung":  null,
    "Akku":  null
}
'@
    'Notebook_Standard.json' = @'
{
    "Format":  "PC-Diagnose-DB/2",
    "Name":  "Referenz: Notebook Standard (Core i5, Intel UHD)",
    "Computer":  "Referenz-Notebook-Standard",
    "Geraet":  {
                   "Id":  "REF-REFERENZ-NOTEBOOK-STANDARD",
                   "Guete":  "hoch",
                   "Quellen":  [
                                   "Referenzsystem"
                               ]
               },
    "Datum":  "2026-10-01 12:00",
    "Version":  "2.95",
    "Quelle":  "Referenz",
    "Module":  "Benchmark",
    "Messdauer":  "normal",
    "System":  "Notebook Standard (Intel Core i5)",
    "Hardware":  {
                     "CPU":  "Intel Core i5-8365U",
                     "Kerne":  4,
                     "Threads":  8,
                     "RAM":  "24 GB DDR4-2400",
                     "GPU":  "UHD Graphics 620",
                     "IGPU":  "UHD Graphics 620",
                     "GPUGemessen":  "Intel(R) UHD Graphics 620",
                     "Datentraeger":  "NVMe PCIe 3.0 SSD 512GB",
                     "Betriebssystem":  "Microsoft Windows 11",
                     "Mainboard":  "Notebook Mainboard",
                     "WindowsInstalliert":  null
                 },
    "Werte":  {
                  "DISK|NVMe3|R8":  112473,
                  "GPU|DWM":  383,
                  "RAM|Latenz":  134.4,
                  "GPU|VMB":  6.5,
                  "CPU|MT":  14744,
                  "GPU|REND":  4.8,
                  "GPU|REND1":  4.7,
                  "DISK|NVMe3|W1":  22297,
                  "RAM|Kopieren":  9.9,
                  "RAM|Schreiben":  13.4,
                  "CPU|ST":  2133,
                  "CPU|AES":  1031,
                  "CPU|SHA":  437,
                  "GPU|RPKT":  442,
                  "RAM|Lesen":  28.3,
                  "DISK|NVMe3|SW":  2424,
                  "DISK|NVMe3|R1":  15440,
                  "DISK|NVMe3|SR":  3487,
                  "CPU|DEFL":  118
              },
    "Messwerte":  {
                      "CPU|Intel(R) Core(TM) i5-8365U CPU @ 1.60GHz|ST":  2133,
                      "CPU|Intel(R) Core(TM) i5-8365U CPU @ 1.60GHz|MT":  14744,
                      "CPU|Intel(R) Core(TM) i5-8365U CPU @ 1.60GHz|AES":  1031,
                      "CPU|Intel(R) Core(TM) i5-8365U CPU @ 1.60GHz|SHA":  437,
                      "CPU|Intel(R) Core(TM) i5-8365U CPU @ 1.60GHz|DEFL":  118,
                      "RAM|24GB|2400|Lesen":  28.3,
                      "RAM|24GB|2400|Schreiben":  13.4,
                      "RAM|24GB|2400|Kopieren":  9.9,
                      "RAM|24GB|2400|Latenz":  134.4,
                      "GPU|Intel(R) UHD Graphics 620|VMB":  6.5,
                      "GPU|Intel(R) UHD Graphics 620|DWM":  383,
                      "GPU|Intel(R) UHD Graphics 620|REND|1280x720":  4.8,
                      "GPU|Intel(R) UHD Graphics 620|REND1|1280x720":  4.7,
                      "GPU|Intel(R) UHD Graphics 620|RPKT":  442,
                      "DISK|0025_3887_11B9_A4F0.|SAMSUNG MZVLB512HBJQ-000L7|SR":  3487,
                      "DISK|0025_3887_11B9_A4F0.|SAMSUNG MZVLB512HBJQ-000L7|SW":  2424,
                      "DISK|0025_3887_11B9_A4F0.|SAMSUNG MZVLB512HBJQ-000L7|R1":  15440,
                      "DISK|0025_3887_11B9_A4F0.|SAMSUNG MZVLB512HBJQ-000L7|R8":  112473,
                      "DISK|0025_3887_11B9_A4F0.|SAMSUNG MZVLB512HBJQ-000L7|W1":  22297,
                      "WINSAT|CPU":  8.9,
                      "WINSAT|RAM":  8.9,
                      "WINSAT|DISK":  9.05,
                      "WINSAT|GFX":  6.2
                  },
    "Ordner":  "",
    "Laufwerke":  [
                      {
                          "Laufwerk":  "SAMSUNG MZVLB512HBJQ-000L7",
                          "Klasse":  "NVMe PCIe 3.0 x4",
                          "SR":  3487,
                          "SW":  2424,
                          "R1":  15440,
                          "R8":  112473,
                          "W1":  22297
                      }
                  ],
    "Befunde":  {
                    "Kritisch":  0,
                    "Warnungen":  0,
                    "Hinweise":  0,
                    "Liste":  [

                              ]
                },
    "Lasttest":  {
                     "CpuTempMax":  86,
                     "GpuTempMax":  74,
                     "CpuWMax":  35,
                     "GpuWMax":  15,
                     "Drosselung":  "Herstellergrenze",
                     "Taktstabilitaet":  88.0,
                     "TaktAbfall":  12.0
                 },
    "Rendertest":  [
                       {
                           "Grafik":  "Intel(R) UHD Graphics 620",
                           "Art":  "iGPU",
                           "Aufloesung":  "1280x720",
                           "Fps":  4.8,
                           "Low1":  4.7,
                           "Punkte":  442,
                           "Bildfehler":  0,
                           "Treiberreset":  false,
                           "Fehler":  ""
                       }
                   ],
    "Schreibzugriffe":  null,
    "Ablauf":  "normal",
    "Sensoren":  {

                 },
    "Optimierung":  null,
    "Akku":  null
}
'@
    'Referenz_Desktop.json' = @'
{
  "Format": "PC-Diagnose-DB/2",
  "Name": "Referenz: Median Desktop-PCs (4 Systeme)",
  "Computer": "Referenz-Desktop-Median",
  "Datum": "2026-10-29",
  "Version": "3.85",
  "Quelle": "Vergleichsdatenbank",
  "Hardware": {
    "CPU": "Desktop Median-Referenz",
    "RAM": "32 GB DDR4/DDR5",
    "GPU": "Desktop-Grafik Median",
    "Datentraeger": "NVMe SSD",
    "Betriebssystem": "Microsoft Windows 11",
    "Mainboard": "Desktop Mainboard",
    "WindowsInstalliert": "",
    "Kerne": 6,
    "Threads": 12
  },
  "Werte": {
    "CPU|AES": 1294,
    "CPU|DEFL": 270,
    "CPU|MT": 35314,
    "CPU|SHA": 2364,
    "CPU|ST": 2912,
    "DISK|HDD|R1": 96,
    "DISK|HDD|R8": 165,
    "DISK|HDD|SR": 121,
    "DISK|HDD|SW": 128,
    "DISK|HDD|W1": 821,
    "DISK|NVMe3|R1": 14090,
    "DISK|NVMe3|R8": 108045,
    "DISK|NVMe3|SR": 3114,
    "DISK|NVMe3|SW": 2910,
    "DISK|NVMe3|W1": 35518,
    "DISK|NVMe4|R1": 17384,
    "DISK|NVMe4|R8": 148780,
    "DISK|NVMe4|SR": 6997,
    "DISK|NVMe4|SW": 4570,
    "DISK|NVMe4|W1": 55940,
    "DISK|SATA-SSD|R1": 9878,
    "DISK|SATA-SSD|R8": 63332,
    "DISK|SATA-SSD|SR": 555,
    "DISK|SATA-SSD|SW": 526,
    "DISK|SATA-SSD|W1": 21824,
    "GPU|DWM": 12006,
    "GPU|REND": 103.8,
    "GPU|REND01": 73.1,
    "GPU|REND1": 67.8,
    "GPU|RPKT": 9571,
    "GPU|STUTTERREL": 14,
    "GPU|VMB": 203.9,
    "RAM|Kopieren": 17.8,
    "RAM|Latenz": 95.1,
    "RAM|Lesen": 45.4,
    "RAM|Schreiben": 21.9
  },
  "Herkunft": {
    "CPU|AES": "Median aus 4 Systemen",
    "CPU|DEFL": "Median aus 4 Systemen",
    "CPU|MT": "Median aus 4 Systemen",
    "CPU|SHA": "Median aus 4 Systemen",
    "CPU|ST": "Median aus 4 Systemen",
    "DISK|HDD|R1": "Median aus 2 Systemen",
    "DISK|HDD|R8": "Median aus 2 Systemen",
    "DISK|HDD|SR": "Median aus 2 Systemen",
    "DISK|HDD|SW": "Median aus 2 Systemen",
    "DISK|HDD|W1": "Median aus 2 Systemen",
    "DISK|NVMe3|R1": "Median aus 3 Systemen",
    "DISK|NVMe3|R8": "Median aus 3 Systemen",
    "DISK|NVMe3|SR": "Median aus 3 Systemen",
    "DISK|NVMe3|SW": "Median aus 3 Systemen",
    "DISK|NVMe3|W1": "Median aus 3 Systemen",
    "DISK|NVMe4|R1": "Median aus 2 Systemen",
    "DISK|NVMe4|R8": "Median aus 2 Systemen",
    "DISK|NVMe4|SR": "Median aus 2 Systemen",
    "DISK|NVMe4|SW": "Median aus 2 Systemen",
    "DISK|NVMe4|W1": "Median aus 2 Systemen",
    "DISK|SATA-SSD|R1": "Median aus 2 Systemen",
    "DISK|SATA-SSD|R8": "Median aus 2 Systemen",
    "DISK|SATA-SSD|SR": "Median aus 2 Systemen",
    "DISK|SATA-SSD|SW": "Median aus 2 Systemen",
    "DISK|SATA-SSD|W1": "Median aus 2 Systemen",
    "GPU|DWM": "Median aus 4 Systemen",
    "GPU|REND": "Median aus 4 Systemen",
    "GPU|REND01": "Median aus 1 Systemen",
    "GPU|REND1": "Median aus 4 Systemen",
    "GPU|RPKT": "Median aus 4 Systemen",
    "GPU|STUTTERREL": "Median aus 1 Systemen",
    "GPU|VMB": "Median aus 4 Systemen",
    "RAM|Kopieren": "Median aus 4 Systemen",
    "RAM|Latenz": "Median aus 4 Systemen",
    "RAM|Lesen": "Median aus 4 Systemen",
    "RAM|Schreiben": "Median aus 4 Systemen"
  },
  "Befunde": {
    "Kritisch": 0,
    "Warnungen": 0,
    "Hinweise": 0,
    "Liste": []
  },
  "Lasttest": {
    "CpuTempMax": 75,
    "GpuTempMax": 68,
    "CpuWMax": 85,
    "GpuWMax": 160,
    "Drosselung": "keine",
    "Taktstabilitaet": 98,
    "TaktAbfall": 2
  }
}
'@
    'Workstation_Mobil.json' = @'
{
    "Format":  "PC-Diagnose-DB/2",
    "Name":  "Referenz: Workstation Mobil (Core i9-13900H, RTX 3000 Ada)",
    "Computer":  "Referenz-Workstation-Mobil",
    "Geraet":  {
                   "Id":  "REF-REFERENZ-WORKSTATION-MOBIL",
                   "Guete":  "hoch",
                   "Quellen":  [
                                   "Referenzsystem"
                               ]
               },
    "Datum":  "2026-10-01 12:00",
    "Version":  "2.95",
    "Quelle":  "Referenz",
    "Module":  "Benchmark",
    "Messdauer":  "normal",
    "System":  "Workstation Mobil (Intel Core i9)",
    "Hardware":  {
                     "CPU":  "13th Gen Intel Core i9-13900H",
                     "Kerne":  14,
                     "Threads":  20,
                     "RAM":  "64 GB LPDDR5-6000",
                     "GPU":  "RTX 3000 Ada Generation Laptop GPU",
                     "IGPU":  "Iris(R) Xe Graphics",
                     "GPUGemessen":  "NVIDIA RTX 3000 Ada Generation Laptop GPU",
                     "Datentraeger":  "NVMe PCIe 4.0 SSD 1TB",
                     "Betriebssystem":  "Microsoft Windows 11",
                     "Mainboard":  "Mobile Workstation Board",
                     "WindowsInstalliert":  null
                 },
    "Werte":  {
                  "GPU|DWM":  2850,
                  "RAM|Latenz":  113.9,
                  "GPU|VMB":  48.4,
                  "CPU|MT":  42759,
                  "GPU|REND":  90.6,
                  "GPU|REND1":  37.8,
                  "RAM|Kopieren":  28.5,
                  "RAM|Schreiben":  47.2,
                  "CPU|ST":  3464,
                  "CPU|AES":  1786,
                  "CPU|SHA":  2409,
                  "GPU|RPKT":  8354,
                  "RAM|Lesen":  67.9,
                  "CPU|DEFL":  295
              },
    "Messwerte":  {
                      "CPU|13th Gen Intel(R) Core(TM) i9-13900H|ST":  3464,
                      "CPU|13th Gen Intel(R) Core(TM) i9-13900H|MT":  42759,
                      "CPU|13th Gen Intel(R) Core(TM) i9-13900H|AES":  1786,
                      "CPU|13th Gen Intel(R) Core(TM) i9-13900H|SHA":  2409,
                      "CPU|13th Gen Intel(R) Core(TM) i9-13900H|DEFL":  295,
                      "RAM|64GB|6000|Lesen":  67.9,
                      "RAM|64GB|6000|Schreiben":  47.2,
                      "RAM|64GB|6000|Kopieren":  28.5,
                      "RAM|64GB|6000|Latenz":  113.9,
                      "GPU|Intel(R) Iris(R) Xe Graphics|VMB":  48.4,
                      "GPU|Intel(R) Iris(R) Xe Graphics|DWM":  2850,
                      "GPU|NVIDIA RTX 3000 Ada Generation Laptop GPU|REND|1280x720":  90.6,
                      "GPU|NVIDIA RTX 3000 Ada Generation Laptop GPU|REND1|1280x720":  37.8,
                      "GPU|NVIDIA RTX 3000 Ada Generation Laptop GPU|RPKT":  8354,
                      "GPU|Intel(R) Iris(R) Xe Graphics|REND|1280x720":  23,
                      "GPU|Intel(R) Iris(R) Xe Graphics|REND1|1280x720":  21.8,
                      "GPU|Intel(R) Iris(R) Xe Graphics|RPKT":  2124,
                      "DISK|E823_8FA6_BF53_0001_001B_444A_4143_AA1D.|NVMe PC SN820 NVMe WD 4096GB|SR":  6689,
                      "DISK|E823_8FA6_BF53_0001_001B_444A_4143_AA1D.|NVMe PC SN820 NVMe WD 4096GB|SW":  3974,
                      "DISK|E823_8FA6_BF53_0001_001B_444A_4143_AA1D.|NVMe PC SN820 NVMe WD 4096GB|R1":  18211,
                      "DISK|E823_8FA6_BF53_0001_001B_444A_4143_AA1D.|NVMe PC SN820 NVMe WD 4096GB|R8":  106690,
                      "DISK|E823_8FA6_BF53_0001_001B_444A_4143_AA1D.|NVMe PC SN820 NVMe WD 4096GB|W1":  46373,
                      "WINSAT|CPU":  9.4,
                      "WINSAT|RAM":  9.4,
                      "WINSAT|DISK":  9.45,
                      "WINSAT|GFX":  8.4
                  },
    "Ordner":  "",
    "Laufwerke":  [
                      {
                          "Laufwerk":  "NVMe PC SN820 NVMe WD 4096GB",
                          "Klasse":  "NVMe",
                          "SR":  6689,
                          "SW":  3974,
                          "R1":  18211,
                          "R8":  106690,
                          "W1":  46373
                      }
                  ],
    "Befunde":  {
                    "Kritisch":  0,
                    "Warnungen":  0,
                    "Hinweise":  0,
                    "Liste":  [

                              ]
                },
    "Lasttest":  {
                     "CpuTempMax":  92,
                     "GpuTempMax":  78,
                     "CpuWMax":  65,
                     "GpuWMax":  70,
                     "Drosselung":  "thermisch",
                     "Taktstabilitaet":  85.0,
                     "TaktAbfall":  15.0
                 },
    "Rendertest":  [
                       {
                           "Grafik":  "NVIDIA RTX 3000 Ada Generation Laptop GPU",
                           "Art":  "dGPU",
                           "Aufloesung":  "1280x720",
                           "Fps":  90.6,
                           "Low1":  37.8,
                           "Punkte":  8354,
                           "Bildfehler":  0,
                           "Treiberreset":  false,
                           "Fehler":  ""
                       },
                       {
                           "Grafik":  "Intel(R) Iris(R) Xe Graphics",
                           "Art":  "iGPU",
                           "Aufloesung":  "1280x720",
                           "Fps":  23,
                           "Low1":  21.8,
                           "Punkte":  2124,
                           "Bildfehler":  0,
                           "Treiberreset":  false,
                           "Fehler":  ""
                       }
                   ],
    "Schreibzugriffe":  null,
    "Ablauf":  "normal",
    "Sensoren":  {

                 },
    "Optimierung":  null,
    "Akku":  null
}
'@
}
