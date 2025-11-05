using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;

namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_ReposicionDB
    {
        private readonly IConfiguration? _config;
        private readonly int module = 9;
        private readonly mSecurityMainDb mSecurity;
        private readonly mTesoreria mTesoreria;


        public frmTES_ReposicionDB(IConfiguration config)
        {
            _config = config;
            mSecurity = new mSecurityMainDb(config);
            mTesoreria = new mTesoreria(config);
        }

        /// <summary>
        /// Obtiene los datos de una solicitud de reposición de tesorería.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDto<tesReposicionData> TES_Reposicion_Obtenet(int CodEmpresa, int solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<tesReposicionData>
            {
                Code = 0,
                Result = new tesReposicionData()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select C.Nsolicitud,C.Codigo,C.beneficiario,C.tipo,C.estado,C.ndocumento,C.id_banco,B.descripcion as BancoX
                                   ,T.descripcion as TipoDocX,C.Monto,C.Fecha_Emision,C.Tipo_Beneficiario, C.Cta_Ahorros
                                   ,C.Detalle1 + ' ' + C.Detalle2 + ' ' + isnull(C.Detalle3 ,'') + ' ' + isnull(C.Detalle4 ,'')  + ' ' + isnull(C.Detalle5 ,'') as 'Detalle'
                                   , case when C.Tipo_Beneficiario = 1 then 'Personas'
                                    when C.Tipo_Beneficiario = 2 then 'Bancos'
                                    when C.Tipo_Beneficiario = 3 then 'Proveedores'
                                    when C.Tipo_Beneficiario = 4 then 'Acreedores' end as 'TipoBeneficiario'
                                   , isnull(C.REPOSICION_IND,0) as 'ReposicionPaso' 
                                    from Tes_Transacciones C inner join Tes_Bancos B on C.id_banco = B.id_Banco
                                    inner join tes_tipos_doc T on C.tipo = T.tipo
                                    inner join tes_banco_docs Y on C.id_banco = Y.id_Banco and C.tipo = Y.tipo
                                    where C.nsolicitud = @solicitud and C.estado in('T','E','I')";

                    response.Result = connection.Query<tesReposicionData>(query,
                        new { solicitud = solicitud }).FirstOrDefault();

                    if(response.Result != null)
                    {
                        response.Result.verificaTag = "S";
                        response.Result.verifica = "----> Este Documento se puede marcar para reponer";
                    }

                    if (response.Result == null)
                    {
                        response.Code = -1;
                        response.Description = "Este documento no es valido para reposición...";
                        response.Result = new tesReposicionData();
                        response.Result.verificaTag = "N";
                        response.Result.verifica = "Este documento no es valido para reposición...";
                        return response;

                    }

                    if (response.Result.reposicionPaso == 1)
                    {
                        response.Result.verificaTag = "N";
                        response.Result.verifica += "Este documento ya Registró Reposición Anteriormente!...";
                    }

                    if (response.Result.tipo_Beneficiario != "3")
                    {
                        response.Result.verificaTag = "N";
                        response.Result.verifica += " - El Tipo de Beneficiario no aplica (Solo Pago de Proveedores)...";
                    }

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
        /// Guarda una solicitud de reposición de tesorería.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto TES_Reposicion_Guardar(int CodEmpresa, tesReposicionData data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select isnull(count(*),0) as Existe from tes_autorizaciones where nombre = @usuarios 
                                     and estado = 'A' and clave = @clave ";

                    var existe = connection.QueryFirstOrDefault<int>(query, new { usuarios = data.usuario, clave = data.clave });
                    if (existe == 0)
                    {
                        response.Code = -1;
                        response.Description = "El usuario y clave de autorización no concuerda con ninguno de los registrados, verifique...";
                        return response;
                    }

                    query = $@"Exec spTES_Reposicion @txtNumeroSolicitud, @glogonUsuario, @usuario, @notas";
                    var result = connection.Execute(query, new
                    {
                        txtNumeroSolicitud = data.nSolicitud,
                        glogonUsuario = data.usuario,
                        usuario = data.usuario,
                        notas = data.notas
                    });

                    //bitácora
                    mTesoreria.sbTesBitacoraEspecial(CodEmpresa, data.nSolicitud, "18", data.notas, data.usuario);
                    
                    mSecurity.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = data.usuario,
                        Modulo = module, // Tesoreria
                        Movimiento = "Aplica - web",
                        DetalleMovimiento = "ReImpresión de Solicitud :" + data.nSolicitud,
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
    }
}
