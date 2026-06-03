using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using static QuestPDF.Helpers.Colors;
using Image = QuestPDF.Infrastructure.Image;
namespace YawgoogIdGenerator
{
    class IdGen
    {
    private struct Card
    {
        public Document card;
        public int cardidx;
    }

     private List<Card> cards = new List<Card>();
     int idCount = 0;
     public int getCount {  get { return idCount; } }
     private byte[]? genImgeBytes(int id)
        {
            if (cards.Count > 0)
            {

                IEnumerable<byte[]> imageArrays = cards[id].card.GenerateImages();
                return imageArrays.ElementAt(0);

            }
            return null;

        }
    public void clearCards()
    {
            idCount = 0;
            cards.Clear();
    }
     public BitmapImage? GetImage(int id)
        {

            var imgData = genImgeBytes((int)id);
            if (imgData != null)
            {
                var image = new BitmapImage();
                using (var mem = new MemoryStream(imgData))
                {
                    mem.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = mem;
                    image.EndInit();
                }
                image.Freeze();
                return image;

            }
            return null;
        }
        public int? genid(Dataman idInformation,int d,int ratioMode)
        {

             QuestPDF.Settings.License = LicenseType.Community;

            string pnouns;
            string pos;
            var colors =  Colors.Black;

            if (File.Exists(idInformation.data[d].Photo)==false)
            {
                return null;
            }
    
            if (idInformation.data[d].Pronouns == "")
            {
                pnouns = "";
            }
            else
            {
                pnouns = $"{idInformation.data[d].Pronouns}";
                pnouns = pnouns.Replace(", ", "/");
            }
            switch (idInformation.data[d].Position)
            {
                case "T":
                case "t":
                    pos = "Three Point Staff";
                    break;

                case "M":
                case "m":
                    pos = "Medicine Bow Staff";
                    break;
                case "S":
                case "s":
                    pos = "Sandy Beach Staff";
                    break;
                default:
                    pos = idInformation.data[d].Position;
                    break;
            }
            switch (idInformation.data[d].Camp)
            {
                case "t":
                case "Yawgoog - Three Point":
                    colors = Colors.Green.Darken3;
                    break;
                case "m":
                case "Yawgoog - Medicine Bow":
                    colors = Colors.Red.Darken3;
                    break;
                case "s":
                case "Yawgoog - Sandy Beach":
                    colors = Colors.Blue.Darken3;
                    break;
                case "r":
                case "Yawgoog - Reservation":
                    colors = Colors.Purple.Darken3;
                    break;
                case "x":
                case "Yawgoog":
                    colors = Colors.Black;
                    break;
                default:
                    colors = Colors.Black;
                    break;
            }

            try
            {
                float width, height;
                switch (ratioMode)
                {
                    case 0:
                        width = 27;
                        height = 41;
                        break;
                    case 1:
                        width = 31;
                        height = 41;
                        break;
                    case 2:
                        width = 33;
                        height = 41;
                        break;
                    case 3:
                        width = 41;
                        height = 41;
                        break;
                    default:
                        width = 41;
                        height = 41;
                        break;
                }
                var assembly = Assembly.GetExecutingAssembly();
                byte[] imageData;

                using (Stream? stream = assembly.GetManifestResourceStream("altlogo"))
                {
                    if (stream == null)
                    {
                        throw new Exception("Critical Error: 'EmbeddedLogo' was not found in the EXE compilation layout.");
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        imageData = ms.ToArray();
                    }
                }

                var currentYear = DateTime.Now.Year.ToString();
                var card = Document.Create(container =>
                    {

                        container.Page(page =>
                        {

                            page.Size(new PageSize(54, 85.6f, Unit.Millimetre));
                            page.DefaultTextStyle(x => x.FontSize(20));
                            page.Content().Column(x =>
                            {
                                //x.Item()
                                //.TranslateY(5, Unit.Millimetre)
                                //.Text(currentYear)
                                //.FontSize(15)
                               // .AlignCenter();
                                //x.Item()
                                //.AlignCenter()
                                //.TranslateY(1, Unit.Millimetre)
                                //.Width(30, Unit.Millimetre)
                                //.Image(imageData);
                                x.Item()
                                .AlignCenter()
                                .TranslateY(6, Unit.Millimetre)

                                .Width(width, Unit.Millimetre)
                                .Height(height, Unit.Millimetre)
                                .Image(Image.FromFile($"{idInformation.data[d].Photo}")).FitUnproportionally();
                               x.Item().TranslateY(6f,Unit.Millimetre).Column(y =>
                               {
                                   y.Item()
                                   .AlignCenter()
                                   .PaddingHorizontal(5)
                                   .Height(18)
                                   .MaxWidth(54, Unit.Millimetre)
                                   .ScaleToFit()
                                   .AlignCenter()
                                   .AlignMiddle()
                                   .Text(idInformation.data[d].First_Name+" " + idInformation.data[d].Last_Name)
                                   .Bold()
                                   .AlignCenter()
                                   .FontSize(18);
                                   y.Item().TranslateY(3).Background(colors).Height(31, Unit.Millimetre).Layers(layers =>
                                   {
                                       layers.PrimaryLayer().AlignMiddle().AlignCenter().TranslateY(-20).Column(z =>
                                       {
                                           z.Spacing(2);

                                           z.Item()
                                           .PaddingHorizontal(5)
                                           .Text($"{pos}")
                                               .FontColor(Colors.White)
                                               .FontSize(13)
                                               .AlignCenter();

                                           z.Item().Text($"{pnouns}")
                                               .AlignCenter()
                                               .FontColor(Colors.White)
                                               .FontSize(9);
                                       });

                                       layers.Layer().AlignBottom().TranslateY(-5).PaddingBottom(2).Row(row =>
                                       {
                                           row.Spacing(0);

                                           row.ConstantItem(15, Unit.Millimetre).TranslateX(10).AlignCenter().Image(imageData).FitWidth();

                                           row.RelativeItem()
                                               .TranslateX(-10)
                                               .AlignMiddle()
                                               .AlignRight()
                                               .Text(DateTime.Now.Year.ToString())
                                               .FontColor(Colors.White)
                                               .FontSize(15)
                                               .Bold();
                                       });
                                   });
                               });
                                //x.Item().Column(y =>
                                //{
                                //    y.Spacing(0);
                                //    y.Item()
                                //    .AlignCenter()
                                //    .PaddingHorizontal(5)
                                //    .Height(18)
                                //    .MaxWidth(54, Unit.Millimetre)
                                //    .ScaleToFit()
                                //    .AlignMiddle()
                                //    .AlignCenter()
                                //    .Text($"{idInformation.data[d].Name} {pnouns}")
                                //    .Bold()
                                //    .AlignCenter()
                                //    .FontSize(18);
                                //    y.Spacing(5);
                                //    y.Item().Background(Colors.Red.Darken3)
                                //    .Padding(5)
                                //    .Height(10,Unit.Millimetre)
                                //    .AlignCenter()
                                //    .ScaleToFit()
                                //    .Width(46, Unit.Millimetre)
                                //    .AlignMiddle()
                                //    .Text($"{pos}").FontColor(Colors.White)
                                //    .AlignCenter()
                                //    .FontSize(10);
                                //    y.Item()
                                    
                                //});

                            }
                            );

                        });

                    });
                Card nCard = new Card();
                nCard.card = card;
                nCard.cardidx = idCount;
                cards.Add(nCard);
                idCount++;
                return idCount - 1;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
                return null;
            }
            
        }
    }
}
