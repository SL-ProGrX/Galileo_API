using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cobros;
using Galileo_API.Models.ProGrX_Hipotecario;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public class FrmVivDesembolsosDb
    {
        private readonly PortalDB _portalDb;
  

        public FrmVivDesembolsosDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmVivDesembolsosDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;

        }

        public ErrorDto<List<OperacionBusquedaDto>> Operaciones_Listar(int codEmpresa)
        {
            var response = new ErrorDto<List<OperacionBusquedaDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
                                SELECT TOP 10
                                    id_solicitud AS operacion,
                                    RTRIM(codigo) AS codigo,
                                    RTRIM(cedula) AS cedula,
                                    montoapr,
                                    saldo
                                FROM reg_creditos
                                WHERE estadosol = 'F'
                                ORDER BY id_solicitud
                            ";

                response.Result = cn.Query<OperacionBusquedaDto>(sql).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Lineas_Listar(int codEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
            SELECT 
                RTRIM(codigo) AS item,
                RTRIM(ISNULL(DESCRIPCION_LINEA, DESCRIPCION)) AS descripcion
            FROM catalogo
            ORDER BY descripcion
        ";

                response.Result = cn.Query<DropDownListaGenericaModel>(sql).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDto<VivDesembolsoHeaderDto> Desembolso_Consultar(int codEmpresa, int operacion)
        {
            var response = new ErrorDto<VivDesembolsoHeaderDto>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"EXEC spCRDViviendaDesembolsoCalculo @operacion";

                response.Result = cn.QueryFirstOrDefault<VivDesembolsoHeaderDto>(
                    sql,
                    new { operacion }
                );
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDto<List<VivDesembolsoDto>> Desembolsos_Listar(int codEmpresa, int operacion)
        {
            var response = new ErrorDto<List<VivDesembolsoDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
                            SELECT 
                                RTRIM(Beneficiario) AS beneficiario,
                                ISNULL(Monto,0) AS monto,
                                ISNULL(disponible,0) AS disponible,
                                InteresesFechaCorte AS fechaCorte,
                                RegistroUsuario AS usuario
                            FROM ViviendaDesembolsos
                            WHERE NumeroOperacion = @operacion
                            ORDER BY CodigoDesembolso DESC
                            ";

                response.Result = cn.Query<VivDesembolsoDto>(
                    sql,
                    new { operacion }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDto<List<VivDesembolsoPendienteDto>> Pendientes_Listar(int codEmpresa, int operacion)
        {
            var response = new ErrorDto<List<VivDesembolsoPendienteDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"SELECT 
                            RTRIM(vdp.Linea) AS linea,
                            vdp.IdContacto AS idcontacto,
                            vdp.IdGarantia AS garantia,
                            RTRIM(vc.Identificacion) AS cedula,
                            RTRIM(vdp.Codigo) AS concepto,
                            RTRIM(vtd.Descripcion) AS descripcion,
                            RTRIM(vdp.Tipo) AS tipo,
                            CASE vdp.Tipo 
                                WHEN 'A' THEN 'Abogado'
                                WHEN 'I' THEN 'Ingeniero'
                                ELSE ''
                            END AS destipo,
                            RTRIM(vdp.Beneficiario) AS beneficiario,
                            ISNULL(vdp.Monto, 0) AS monto,
                            ISNULL(vdp.Descuento, 0) AS descuento,
                            ISNULL(vdp.MontoGiro, 0) AS montogiro,
                            RTRIM(vdp.CodigoCuenta) AS cuenta,
                            CASE vdp.AplicaIntereses
                                WHEN 0 THEN 'NO'
                                WHEN 1 THEN 'SI'
                                ELSE ''
                            END AS aplicainteres,
                            RTRIM(vdp.Usuario) AS usuario,
                            vdp.Fecha AS fecha
                        FROM viviendaGarantia vg
                        INNER JOIN ViviendaDesembolsosPendientes vdp
                            ON vg.IdGarantia = vdp.IdGarantia
                        INNER JOIN ViviendaTiposDesembolsos vtd
                            ON vdp.Codigo = vtd.Codigo
                        INNER JOIN ViviendaContactos vc
                            ON vdp.IdContacto = vc.IdContacto
                        WHERE vdp.estado = 'P'
                          AND vg.NumeroOperacion = @operacion
                        ORDER BY vdp.Fecha DESC
                                    ";

                response.Result = cn.Query<VivDesembolsoPendienteDto>(
                    sql,
                    new { operacion }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Bancos_Listar(int codEmpresa, string usuario)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"exec spCrd_SGT_Bancos @usuario";

                response.Result = cn.Query<short, string, DropDownListaGenericaModel>(
                    sql,
                    (idx, itmx) => new DropDownListaGenericaModel
                    {
                        item = idx.ToString(),
                        descripcion = itmx
                    },
                    new { usuario },
                    splitOn: "ITMX"
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cuentas_Listar(int codEmpresa, string cedula, int bancoId)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"exec spSys_Cuentas_Bancarias @cedula, @bancoId, 1";

                response.Result = cn.Query<DropDownListaGenericaModel>(
                    sql,
                    new { cedula, bancoId }
                ).ToList();
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
