using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;

namespace Galileo_API.DataBaseTier
{
    public class FrmTesAnulacionDocDb
    {
        private readonly MTesoreria mTesoreria;
        private readonly MSecurityMainDb mSecurityMainDb;
        private readonly int vModulo = 9; // Módulo de Tesorería
        private readonly PortalDB _portalDB;

        public FrmTesAnulacionDocDb(IConfiguration config)
        {
            mTesoreria = new MTesoreria(config);
            mSecurityMainDb = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtengo la solicitud de anulación de documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDto<TesAnulacionDocData> TES_Anulacion_Obtener(int CodEmpresa, int solicitud, string usuario)
        {

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"select C.Nsolicitud,C.tipo,C.estado,C.ndocumento,C.id_banco,B.descripcion as BancoX
                                   ,T.descripcion as TipoDocX,C.detalle_Anulacion,C.Estado_Asiento,C.Fecha_emision
                                    from Tes_Transacciones C inner join Tes_Bancos B on C.id_banco = B.id_Banco
                                    inner join  tes_tipos_doc T on C.tipo = T.tipo
                                    where C.nsolicitud = @solicitud ";

                var response = conn.Query<TesAnulacionDocData>(query,
                        new
                        {
                            solicitud = solicitud
                        }).FirstOrDefault() ?? new TesAnulacionDocData();

                response.verifica = mTesoreria.fxTesTipoAccesoValida(
                   CodEmpresa,
                   response.id_banco ?? string.Empty,
                   usuario,
                   response.tipo ?? string.Empty,
                   "N").Result;

                return response;
            });
        }

        /// <summary>
        /// Anula un Documento ya emitido y actualiza saldos del Banco.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="anula"></param>
        /// <returns></returns>
        public ErrorDto TES_Anulacion_Anular(int CodEmpresa, string usuario , TesAnulacionAnulaModel anula)
        {
            /*
             *  ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
                'OBJETIVO:      Anula un Documento ya emitido y actualiza saldos del Banco.
                'REFERENCIAS:   Bitacora - (Registra movimientos sobre la Base de Datos)
                '               LimpiaObjetos - (Limpia los objetos de entrada de datos)
                'OBSERVACIONES: Ninguna.
                ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            */
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                int vCopia = 0;
                vCopia = anula.copia == true ? 1 : 0;

                var query = $@"exec spTES_Transaccion_Anula @TesoreriaId, @Notas,@Usuario,@Copia, @ConceptoId ";
                conn.Execute(query, new
                {
                    TesoreriaId = anula.nsolicitud,
                    Notas = anula.notas,
                    Usuario = anula.usuario,
                    Copia = vCopia,
                    ConceptoId = anula.cod_concepto_anulacion
                });

                //Bitácora
                string detalleBitacora = $"Anula Solicitud : {anula.nsolicitud} - {anula.notas} - {anula.cod_concepto_anulacion}";
                mSecurityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = detalleBitacora,
                    Movimiento = "Anula - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }

            return DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// Duplica una determinada solicitud ya ingresada a Tesoreria. Tambien duplica
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="anula"></param>
        /// <returns></returns>
        public ErrorDto TES_AnulacionCopiaSolicitud(int CodEmpresa, string usuario, TesAnulacionAnulaModel anula)
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
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
               
                var query = $@"exec spTES_Transaccion_Copia @TesoreriaId, @Notas, @Usuario ";
                conn.Execute(query, new
                {
                    TesoreriaId = anula.nsolicitud,
                    Notas = anula.notas,
                    Usuario = anula.usuario
                });

                //Bitácora
                mSecurityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Copia Solicitud : {anula.nsolicitud} - {anula.notas}",
                    Movimiento = "Aplica - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }

            return DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// Metodo que obtiene los conceptos de anulación
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_AnulacionConceptos_Obtener(int CodEmpresa, string tipo)
        {

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select ID_CONCEPTO_ANULA as 'item', DESCRIPCION  FROM TES_ANULA_CONCEPTOS 
                                       WHERE TIPO = @tipo AND ACTIVO = 1";

                return conn.Query<DropDownListaGenericaModel>(query, new { tipo }).ToList();
            });
        }
    }
}
