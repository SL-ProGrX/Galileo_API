using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSifOficinasMetasBD
    {
        private readonly IConfiguration _config;


        public FrmSifOficinasMetasBD(IConfiguration config)
        {
            _config = config;
        }


        /// <summary>
        /// Consulta de todas las metas 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="oficina"></param>
        /// <param name="anio"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<SifOficinasMetaLista> Sif_OficinasMetasLista_Obtener(int CodEmpresa, string oficina, int anio, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<SifOficinasMetaLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new SifOficinasMetaLista()
                {
                    total = 0,
                    lista = new List<SifOficinasMetaData>()
                }
            };

            try
            {

                using var connection = new SqlConnection(stringConn);

                const string sp = "spSIFOficinaMetasPeriodo";
                result.Result.lista = connection.Query<SifOficinasMetaData>(sp, new
                {
                    oficina,
                    anio_inicio = anio,
                    anio_corte = anio + 1,
                    usuario
                }, commandType: CommandType.StoredProcedure).ToList();

                result.Result.total = result.Result.lista.Count;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<SifOficinasMetaData>();
            }
            return result;
        }


        /// <summary>
        /// Consulta los periodos por oficina
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="oficina"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Sif_OficinasMetasPeriodos_Obtener(int CodEmpresa, string oficina)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);

                const string query = @"select Anio_Inicio as 'item',CONCAT(Anio_Inicio, ' - ', ANIO_CORTE) as descripcion
from sif_oficina_metas_periodos
where cod_oficina = @cod_oficina
order by anio_Corte desc";

                result.Result = connection.Query<DropDownListaGenericaModel>(query, new { cod_oficina = oficina }).ToList();

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Actualiza las metas por oficina y año
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="oficina"></param>
        /// <param name="usuario"></param>
        /// <param name="metas"></param>
        /// <returns></returns>
        public ErrorDto Sif_OficinasMetas_Actualizar(int CodEmpresa, string oficina, string usuario, List<SifOficinasMetaData> metas)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                foreach (var meta in metas)
                {
                    const string query = @"UPDATE sif_oficina_metas
                                    SET mes_meta = @mes_meta,
                                        acumulado_meta = @acumulado_meta,
                                        Actualizado_Fecha = dbo.MyGetdate(),
                                        Actualizado_Usuario = @usuario
                                    WHERE cod_oficina = @cod_oficina  and Anio = @anio and Mes =@mes";
                    connection.Execute(query, new
                    {
                        mes_meta = meta.mes_meta,
                        acumulado_meta = meta.acumulado_meta,
                        usuario = usuario,
                        cod_oficina = oficina,
                        anio = meta.anio,
                        mes = meta.mes
                    });


                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

    }
}