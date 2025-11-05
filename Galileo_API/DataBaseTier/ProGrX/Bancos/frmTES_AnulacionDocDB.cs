using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.BusinessLogic;
using PgxAPI.Models;
using PgxAPI.Models.CPR;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;
using PgxAPI.Models.TES;
using System.Reflection;

namespace PgxAPI.DataBaseTier
{
    public class frmTES_AnulacionDocDB
    {
        private readonly IConfiguration? _config;
        private readonly mTesoreria mTesoreria;
        private readonly mSecurityMainDb mSecurityMainDb;
        private readonly int vModulo = 9; // Módulo de Tesorería

        public frmTES_AnulacionDocDB(IConfiguration config)
        {
            _config = config;
            mTesoreria = new mTesoreria(config);
            mSecurityMainDb = new mSecurityMainDb(config);
        }

        /// <summary>
        /// Obtengo la solicitud de anulación de documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDTO<TES_AnulacionDocData> TES_Anulacion_Obtener(int CodEmpresa, int solicitud, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<TES_AnulacionDocData>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select C.Nsolicitud,C.tipo,C.estado,C.ndocumento,C.id_banco,B.descripcion as BancoX
                                   ,T.descripcion as TipoDocX,C.detalle_Anulacion,C.Estado_Asiento,C.Fecha_emision
                                    from Tes_Transacciones C inner join Tes_Bancos B on C.id_banco = B.id_Banco
                                    inner join  tes_tipos_doc T on C.tipo = T.tipo
                                    where C.nsolicitud = @solicitud ";

                    response.Result = connection.Query<TES_AnulacionDocData>(query,
                        new
                        {
                            solicitud = solicitud
                        }).FirstOrDefault();

                    if (response.Result != null)
                    {
                        response.Result.verifica = mTesoreria.fxTesTipoAccesoValida(CodEmpresa, response.Result.id_banco, usuario, response.Result.tipo , "N").Result;
                    }
                    else
                    {
                        response.Result = new TES_AnulacionDocData();
                        response.Result.verifica = false;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Anula un Documento ya emitido y actualiza saldos del Banco.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="anula"></param>
        /// <returns></returns>
        public ErrorDTO TES_Anulacion_Anular(int CodEmpresa, string usuario ,TES_AnulacionAnulaModel anula)
        {
            /*
             *  ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
                'OBJETIVO:      Anula un Documento ya emitido y actualiza saldos del Banco.
                'REFERENCIAS:   Bitacora - (Registra movimientos sobre la Base de Datos)
                '               LimpiaObjetos - (Limpia los objetos de entrada de datos)
                'OBSERVACIONES: Ninguna.
                ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            */
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    int vCopia = 0;
                    vCopia = anula.copia == true ? 1 : 0;

                    var query = $@"exec spTES_Transaccion_Anula {anula.nsolicitud}, '{anula.notas}', '{anula.usuario}', {vCopia}, {anula.cod_concepto_anulacion} ";
                    connection.Execute(query);

                    //Bitácora

                    string detalleBitacora = $"Anula Solicitud : {anula.nsolicitud} - {anula.notas} - {anula.cod_concepto_anulacion}";

        
                    mSecurityMainDb.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = detalleBitacora,
                        Movimiento = "Anula - WEB",
                        Modulo = vModulo
                    });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Duplica una determinada solicitud ya ingresada a Tesoreria. Tambien duplica
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="anula"></param>
        /// <returns></returns>
        public ErrorDTO TES_AnulacionCopiaSolicitud(int CodEmpresa, string usuario, TES_AnulacionAnulaModel anula)
        {
            /*
                ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
                'OBJETIVO:      Duplica una determinada solicitud ya ingresada a Tesoreria. Tambien duplica
                '               el detalle de la misma solicitud para la nueva.
                'REFERENCIAS:   Bitacora - (Registra movimientos sobre la Base de Datos)
                '
                '               fxFechaServidor - (Devuelve la fecha del servidor)
                'OBSERVACIONES: Ninguna.
                ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
             */
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    int vCopia = 0;
                    vCopia = anula.copia == true ? 1 : 0;

                    var query = $@"exec spTES_Transaccion_Copia {anula.nsolicitud}, '{anula.notas}', '{anula.usuario}' ";
                    connection.Execute(query);

                    //Bitácora
                    mSecurityMainDb.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Copia Solicitud : {anula.nsolicitud} - {anula.notas}",
                        Movimiento = "Aplica - WEB",
                        Modulo = vModulo
                    });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDTO<List<DropDownListaGenericaModel>> TES_AnulacionConceptos_Obtener(int CodEmpresa, string tipo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "",
                Result = new List<DropDownListaGenericaModel>(),
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select ID_CONCEPTO_ANULA as 'item', DESCRIPCION  FROM TES_ANULA_CONCEPTOS  WHERE TIPO = @tipo AND ACTIVO = 1";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query, new { tipo }).ToList();
                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }
    }
}
