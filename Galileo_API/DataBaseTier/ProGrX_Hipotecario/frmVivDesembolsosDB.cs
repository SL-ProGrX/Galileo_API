using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cobros;
using Galileo_API.Models.ProGrX_Hipotecario;
using Microsoft.Data.SqlClient;
using System;

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

        private ErrorDto<T> ExecuteQuery<T>(int codEmpresa, Func<SqlConnection, T> query)
        {
            var response = new ErrorDto<T>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = query(cn);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDto<List<OperacionBusquedaDto>> Operaciones_Listar(int codEmpresa)
        {
            return ExecuteQuery(codEmpresa, cn =>
            {
                const string sql = @"
                    SELECT TOP 10
                        id_solicitud AS operacion,
                        RTRIM(codigo) AS codigo,
                        RTRIM(cedula) AS cedula,
                        montoapr,
                        saldo
                    FROM reg_creditos
                    WHERE estadosol = 'F'
                    ORDER BY id_solicitud";

                return cn.Query<OperacionBusquedaDto>(sql).ToList();
            });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Lineas_Listar(int codEmpresa)
        {
            return ExecuteQuery(codEmpresa, cn =>
            {
                const string sql = @"
                    SELECT
                        RTRIM(codigo) AS item,
                        RTRIM(ISNULL(DESCRIPCION_LINEA, DESCRIPCION)) AS descripcion
                    FROM catalogo
                    ORDER BY descripcion";

                return cn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        public ErrorDto<VivDesembolsoHeaderDto> Desembolso_Consultar(int codEmpresa, int operacion)
        {
            return ExecuteQuery(codEmpresa, cn =>
            {
                const string sql = @"EXEC spCRDViviendaDesembolsoCalculo @operacion";

                return cn.QueryFirstOrDefault<VivDesembolsoHeaderDto>(
                          sql,
                          new { operacion }
                      ) ?? new VivDesembolsoHeaderDto();
            });
        }

        public ErrorDto<List<VivDesembolsoDto>> Desembolsos_Listar(int codEmpresa, int operacion)
        {
            return ExecuteQuery(codEmpresa, cn =>
            {
                const string sql = @"
                    SELECT
                        RTRIM(Beneficiario) AS beneficiario,
                        ISNULL(Monto, 0) AS monto,
                        ISNULL(disponible, 0) AS disponible,
                        InteresesFechaCorte AS fechaCorte,
                        RegistroUsuario AS usuario
                    FROM ViviendaDesembolsos
                    WHERE NumeroOperacion = @operacion
                    ORDER BY CodigoDesembolso DESC";

                return cn.Query<VivDesembolsoDto>(
                    sql,
                    new { operacion }).ToList();
            });
        }

        public ErrorDto<List<VivDesembolsoPendienteDto>> Pendientes_Listar(int codEmpresa, int operacion)
        {
            return ExecuteQuery(codEmpresa, cn =>
            {
                const string sql = @"
                    SELECT
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
                    ORDER BY vdp.Fecha DESC";

                return cn.Query<VivDesembolsoPendienteDto>(
                    sql,
                    new { operacion }).ToList();
            });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Bancos_Listar(int codEmpresa, string usuario)
        {
            return ExecuteQuery(codEmpresa, cn =>
            {
                const string sql = @"EXEC spCrd_SGT_Bancos @usuario";

                return cn.Query<short, string, DropDownListaGenericaModel>(
                    sql,
                    (idx, itmx) => new DropDownListaGenericaModel
                    {
                        item = idx.ToString(),
                        descripcion = itmx
                    },
                    new { usuario },
                    splitOn: "ITMX").ToList();
            });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cuentas_Listar(int codEmpresa, string bancoId)
        {
            return ExecuteQuery(codEmpresa, cn =>
            {
                const string sql = @"EXEC spSys_Cuentas_Bancarias '', @bancoId, 1";

                return cn.Query<string, string, DropDownListaGenericaModel>(
                    sql,
                    (id, desc) => new DropDownListaGenericaModel
                    {
                        item = id.ToString(),
                        descripcion = desc
                    },
                    new { bancoId },
                    splitOn: "ITMX"
                ).ToList();
            });
        }


        public ErrorDto<List<ConceptoApiDto>> Conceptos_Listar(int codEmpresa)
        {
            return ExecuteQuery(codEmpresa, cn =>
            {
                const string sql = @"
                            SELECT RTRIM(codigo) AS item,RTRIM(descripcion) AS descripcion,
                    ISNULL(aplicaInteres, 0) AS aplicaIntereses
                FROM ViviendaTiposDesembolsos";

                return cn.Query<ConceptoApiDto>(sql).ToList();
            });
        }
        public ErrorDto<bool> PermiteDesembolso(int codEmpresa,int operacion,int index)
        {
            var response = new ErrorDto<bool>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                string sql;

                if (index == 2) 
                {
                    sql = @"
                SELECT COUNT(*) 
                FROM REG_CREDITOS
                WHERE ESTADOSOL = 'F'
                AND ID_SOLICITUD = @Operacion
            ";
                }
                else 
                {
                    sql = @"
                SELECT COUNT(*)
                FROM REG_CREDITOS R
                INNER JOIN ViviendaDesembolsosDisponible D
                    ON R.ID_SOLICITUD = D.NumeroOperacion
                WHERE R.ESTADOSOL = 'F'
                AND R.ID_SOLICITUD = @Operacion
                AND R.EMITIR NOT IN ('CK','TE')
                AND D.Disponible > 0
            ";
                }

                var result = cn.ExecuteScalar<int>(sql, new
                {
                    Operacion = operacion
                });

                response.Result = result > 0;
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