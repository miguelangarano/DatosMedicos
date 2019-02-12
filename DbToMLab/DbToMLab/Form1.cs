using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace DbToMLab
{
    public partial class Form1 : Form
    {
        string connStr;
        OleDbConnection MyConn;

        public Form1()
        {
            InitializeComponent();
            connStr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:/Users/Miguel/Desktop/Laboratorio971.mdb;Persist Security Info=True;User ID=admin";
            MyConn = new OleDbConnection(connStr);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MyConn.Open();
            string idcabecera = textBox1.Text;
            if (idcabecera!=null && idcabecera!="")
            {
                
                string StrCmd = "SELECT * FROM CabeceraRegistro WHERE IDRegistro=" + idcabecera + "";
                OleDbCommand Cmd = new OleDbCommand(StrCmd, MyConn);
                OleDbDataReader ObjReader = Cmd.ExecuteReader();

                if (ObjReader != null)
                {
                    while (ObjReader.Read())
                    {
                        textBox2.Text=ObjReader[1].ToString();
                        textBox3.Text = ObjReader[2].ToString();
                    }
                    button2.Enabled = true;
                }
                else
                {
                    button2.Enabled = false;
                }
            }
            MyConn.Close();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            MyConn.Open();
            CabeceraRegistro cabeceraRegistro = getCabeceraRegistro();
            List<Resultados> listaResultados = getResultados(cabeceraRegistro.IDRegistro);
            List<Determinaciones> listaDeterminaciones = getDeterminaciones(listaResultados);
            List<Grupos> listaGrupos = getGrupos(listaDeterminaciones);
            List<RangosReferencia> listaRangos = getRangos(listaDeterminaciones);
            List<Observaciones> listaObservaciones = getObservaciones(cabeceraRegistro.IDRegistro);
            MyConn.Close();

            try
            {
                string connectstring = "mongodb://miguelangarano:miguel123@ds127015.mlab.com:27015/datosmedicos";
                MongoClient client = new MongoClient(connectstring);
                var db = client.GetDatabase("datosmedicos");
                var collection = db.GetCollection<BsonDocument>("datos");

                Console.WriteLine(listaGrupos.Count);

                List<BsonDocument> listDocumentsGrupos = new List<BsonDocument>();
                foreach(var item in listaGrupos)
                {
                    List<BsonDocument> listDocumentsResultados = new List<BsonDocument>();
                    foreach (var item1 in listaResultados)
                    {
                        List<BsonDocument> listDocumentsRangos = new List<BsonDocument>();
                        foreach(var item2 in listaRangos.FindAll(x => x.IDDeterminacion == item1.IDDeterminacion))
                        {
                            var documentRangos = new BsonDocument
                            {
                                {"Descripcion", item2.Descripcion },
                                {"Rango", item2.Rango },
                                {"Unidad", item2.Unidad }
                            };
                            listDocumentsRangos.Add(documentRangos);
                        }


                        List<BsonDocument> listDocumentsObservaciones = new List<BsonDocument>();
                        foreach (var item3 in listaObservaciones.FindAll(x => x.IDRegistro == cabeceraRegistro.IDRegistro && x.IDGrupo == item.IDGrupo))
                        {
                            var documentObservaciones = new BsonDocument
                            {
                                {"Observacion", item3.Observacion },
                            };
                            listDocumentsObservaciones.Add(documentObservaciones);
                        }



                        var documentResultados = new BsonDocument
                        {
                            {"Nombre", listaDeterminaciones.Find(x => x.IDDeterminacion == item1.IDDeterminacion).Nombre },
                            {"Abreviatura", listaDeterminaciones.Find(x => x.IDDeterminacion == item1.IDDeterminacion).Abreviatura },
                            {"Unidad", listaDeterminaciones.Find(x => x.IDDeterminacion == item1.IDDeterminacion).Unidad },
                            {"ValorResultado", item1.ValorResultado },
                            {"Muestra", item1.Muestra },
                            {"Observacion", item1.Observacion },
                            {"RangosReferencia", new BsonArray(listDocumentsRangos) },
                            {"Observaciones", new BsonArray(listDocumentsObservaciones) }
                        };
                        listDocumentsResultados.Add(documentResultados);
                    }

                    var documentGrupos = new BsonDocument
                    {
                        {"Nombre", item.Nombre },
                        {"Muestra", item.Muestra },
                        {"OrdenImpresion", item.OrdenImpresion },
                        {"Resultados", new BsonArray(listDocumentsResultados) }
                    };
                    listDocumentsGrupos.Add(documentGrupos);
                }

                var documentCabecera = new BsonDocument {
                    {"IDRegistro", cabeceraRegistro.IDRegistro },
                    {"Paciente", cabeceraRegistro.Paciente },
                    {"Fecha", cabeceraRegistro.Fecha },
                    {"Costo", cabeceraRegistro.Costo },
                    {"Medico", cabeceraRegistro.Medico },
                    {"Sexo", cabeceraRegistro.Sexo },
                    {"Edad", cabeceraRegistro.Edad },
                    {"DireccionOrigen", cabeceraRegistro.DireccionOrigen },
                    {"TelefonoOrigen", cabeceraRegistro.TelefonoOrigen },
                    {"Impreso", cabeceraRegistro.Impreso },
                    {"Grupos", new BsonArray(listDocumentsGrupos) },
                };
                await collection.InsertOneAsync(documentCabecera);
            }
            catch (Exception ex)
            {
                Console.WriteLine("errorrrrr: " + ex.Message);
            }
        }

        private CabeceraRegistro getCabeceraRegistro()
        {
            string idcabecera = textBox1.Text;
            CabeceraRegistro cabeceraRegistro = new CabeceraRegistro();
            if (idcabecera != null && idcabecera != "")
            {

                string StrCmd = "SELECT * FROM CabeceraRegistro WHERE IDRegistro=" + idcabecera + "";
                OleDbCommand Cmd = new OleDbCommand(StrCmd, MyConn);
                OleDbDataReader ObjReader = Cmd.ExecuteReader();

                if (ObjReader != null)
                {
                    while (ObjReader.Read())
                    {
                        cabeceraRegistro = new CabeceraRegistro()
                        {
                            IDRegistro = int.Parse(ObjReader[0].ToString()),
                            Paciente = ObjReader[1].ToString(),
                            Fecha = ObjReader[2].ToString(),
                            Costo = double.Parse(ObjReader[3].ToString()),
                            Medico = ObjReader[4].ToString(),
                            Sexo = ObjReader[5].ToString(),
                            Edad = int.Parse(ObjReader[6].ToString()),
                            DireccionOrigen = ObjReader[7].ToString(),
                            TelefonoOrigen = ObjReader[8].ToString(),
                            Impreso = bool.Parse(ObjReader[9].ToString())
                        };
                    }

                    return cabeceraRegistro;
                }
                else
                {
                    return cabeceraRegistro;
                }
            }
            else
            {
                return cabeceraRegistro;
            }
        }

        private List<Resultados> getResultados(int IDRegistro)
        {
            string StrCmd = "SELECT * FROM Resultados WHERE IDRegistro=" + IDRegistro + "";
            OleDbCommand Cmd = new OleDbCommand(StrCmd, MyConn);
            OleDbDataReader ObjReader = Cmd.ExecuteReader();
            List<Resultados> listaResultados = new List<Resultados>();

            if (ObjReader != null)
            {
                while (ObjReader.Read())
                {
                    Resultados resultados = new Resultados()
                    {
                        IDRegistro = int.Parse(ObjReader[0].ToString()),
                        IDDeterminacion = int.Parse(ObjReader[1].ToString()),
                        ValorResultado = ObjReader[2].ToString(),
                        Muestra = ObjReader[3].ToString(),
                        Observacion = ObjReader[4].ToString()
                    };

                    listaResultados.Add(resultados);
                }

                return listaResultados;
            }
            else
            {
                return listaResultados;
            }

        }

        private List<Determinaciones> getDeterminaciones(List<Resultados> resultados)
        {
            List<Determinaciones> lista = new List<Determinaciones>();

            foreach (var item in resultados)
            {
                string StrCmd = "SELECT * FROM Determinaciones WHERE IDDeterminacion=" + item.IDDeterminacion.ToString() + "";
                OleDbCommand Cmd = new OleDbCommand(StrCmd, MyConn);
                OleDbDataReader ObjReader = Cmd.ExecuteReader();

                if (ObjReader != null)
                {
                    while (ObjReader.Read())
                    {
                        Determinaciones determinaciones = new Determinaciones()
                        {
                            IDDeterminacion = int.Parse(ObjReader[0].ToString()),
                            IDGrupo = int.Parse(ObjReader[1].ToString()),
                            Subgrupo = ObjReader[2].ToString(),
                            Nombre = ObjReader[3].ToString(),
                            Abreviatura = ObjReader[4].ToString(),
                            Comentario = ObjReader[5].ToString(),
                            Unidad = ObjReader[6].ToString(),
                            OrdenImpresion = ObjReader[7].ToString()
                        };

                        lista.Add(determinaciones);
                    }
                }
                else
                {
                    return lista;
                }
            }

            return lista;
        }

        private List<Grupos> getGrupos(List<Determinaciones> determinaciones)
        {
            List<Grupos> listaGrupos = new List<Grupos>();

            foreach(var item in determinaciones)
            {
                string StrCmd = "SELECT * FROM Grupos WHERE IDGrupo=" + item.IDGrupo.ToString() + "";
                OleDbCommand Cmd = new OleDbCommand(StrCmd, MyConn);
                OleDbDataReader ObjReader = Cmd.ExecuteReader();

                if (ObjReader != null)
                {
                    while (ObjReader.Read())
                    {
                        Grupos grupos = new Grupos()
                        {
                            IDGrupo = int.Parse(ObjReader[0].ToString()),
                            Nombre = ObjReader[1].ToString(),
                            Muestra = ObjReader[2].ToString(),
                            OrdenImpresion = ObjReader[3].ToString(),
                            Comentario = ObjReader[4].ToString()
                        };
                        listaGrupos.Add(grupos);
                    }
                }
                else
                {
                    return listaGrupos;
                }
            }

            return listaGrupos;
        }

        private List<RangosReferencia> getRangos(List<Determinaciones> determinaciones)
        {
            List<RangosReferencia> listaRangos = new List<RangosReferencia>();

            foreach (var item in determinaciones)
            {
                string StrCmd = "SELECT * FROM RangosReferencia WHERE IDDeterminacion=" + item.IDDeterminacion.ToString() + "";
                OleDbCommand Cmd = new OleDbCommand(StrCmd, MyConn);
                OleDbDataReader ObjReader = Cmd.ExecuteReader();

                if (ObjReader != null)
                {
                    while (ObjReader.Read())
                    {
                        RangosReferencia rangos = new RangosReferencia()
                        {
                            Serial = int.Parse(ObjReader[0].ToString()),
                            Descripcion = ObjReader[1].ToString(),
                            Rango = ObjReader[2].ToString(),
                            Unidad = ObjReader[3].ToString(),
                            Observacion = ObjReader[4].ToString(),
                            IDDeterminacion = int.Parse(ObjReader[5].ToString())
                        };
                        listaRangos.Add(rangos);
                    }
                }
                else
                {
                    return listaRangos;
                }
            }

            return listaRangos;
        }

        private List<Observaciones> getObservaciones(int IDRegistro)
        {
            List<Observaciones> listaObservaciones = new List<Observaciones>();
            string StrCmd = "SELECT * FROM Observaciones WHERE IDRegistro=" + IDRegistro + "";
            OleDbCommand Cmd = new OleDbCommand(StrCmd, MyConn);
            OleDbDataReader ObjReader = Cmd.ExecuteReader();

            if (ObjReader != null)
            {
                while (ObjReader.Read())
                {
                    Observaciones observaciones = new Observaciones()
                    {
                        IDRegistro = int.Parse(ObjReader[0].ToString()),
                        IDGrupo = int.Parse(ObjReader[1].ToString()),
                        Observacion = ObjReader[2].ToString()
                    };

                    listaObservaciones.Add(observaciones);
                }

                return listaObservaciones;
            }
            else
            {
                return listaObservaciones;
            }

        }

    }

    public class CabeceraRegistro
    {
        public int IDRegistro { get; set; }
        public string Paciente { get; set; }
        public string Fecha { get; set; }
        public double Costo { get; set; }
        public string Medico { get; set; }
        public string Sexo { get; set; }
        public int Edad { get; set; }
        public string DireccionOrigen { get; set; }
        public string TelefonoOrigen { get; set; }
        public bool Impreso { get; set; }
    }

    public class Resultados
    {
        public int IDRegistro { get; set; }
        public int IDDeterminacion { get; set; }
        public string ValorResultado { get; set; }
        public string Muestra { get; set; }
        public string Observacion { get; set; }
    }

    public class Determinaciones
    {
        public int IDDeterminacion { get; set; }
        public int IDGrupo { get; set; }
        public string Subgrupo { get; set; }
        public string Nombre { get; set; }
        public string Abreviatura { get; set; }
        public string Comentario { get; set; }
        public string Unidad { get; set; }
        public string OrdenImpresion { get; set; }
    }

    public class Grupos
    {
        public int IDGrupo { get; set; }
        public string Nombre { get; set; }
        public string Muestra { get; set; }
        public string OrdenImpresion { get; set; }
        public string Comentario { get; set; }
    }

    public class RangosReferencia
    {
        public int Serial { get; set; }
        public string Descripcion { get; set; }
        public string Rango { get; set; }
        public string Unidad { get; set; }
        public string Observacion { get; set; }
        public int IDDeterminacion { get; set; }
    }

    public class Observaciones
    {
        public int IDRegistro { get; set; }
        public int IDGrupo { get; set; }
        public string Observacion { get; set; }
    }

}