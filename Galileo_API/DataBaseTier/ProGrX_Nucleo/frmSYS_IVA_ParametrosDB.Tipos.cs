using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public partial class FrmSysIvaParametrosDB
    {
        public ErrorDto<List<TipoIvaFactura>> Sys_Iva_Tipos_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<TipoIvaFactura>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<TipoIvaFactura>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                result.Result = connection.Query<TipoIvaFactura>(
                    "dbo.spSys_IVA_TIPOS_List",
                    commandType: CommandType.StoredProcedure).ToList();
                CompletarDatosCuentaIva(connection, result.Result);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new List<TipoIvaFactura>();
            }

            return result;
        }

        public ErrorDto<TipoIvaFactura> Sys_Iva_Tipo_Insertar(
            int CodEmpresa,
            string usuario,
            SysIvaTipoSaveRequest request)
        {
            return GuardarTipoIva(CodEmpresa, null, usuario, request, true);
        }

        public ErrorDto<TipoIvaFactura> Sys_Iva_Tipo_Actualizar(
            int CodEmpresa,
            string codIVA,
            string usuario,
            SysIvaTipoSaveRequest request)
        {
            return GuardarTipoIva(CodEmpresa, codIVA, usuario, request, false);
        }

        private ErrorDto<TipoIvaFactura> GuardarTipoIva(
            int CodEmpresa,
            string? codigoExistente,
            string usuario,
            SysIvaTipoSaveRequest request,
            bool insertar)
        {
            string codigo = (insertar ? request.Cod_IVA : codigoExistente)?.Trim() ?? string.Empty;
            string descripcion = request.Descripcion?.Trim() ?? string.Empty;
            string cuenta = request.Cod_Cuenta?.Trim() ?? string.Empty;

            var errorValidacion = ValidarDatosTipoIva(codigo, descripcion, cuenta, request);
            if (errorValidacion is not null)
                return errorValidacion;

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                connection.Open();
                using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);

                int existe = connection.ExecuteScalar<int>(
                    "SELECT COUNT(1) FROM SYS_IVA_TIPOS WHERE COD_IVA = @Codigo",
                    new { Codigo = codigo },
                    transaction);

                if (insertar && existe > 0)
                    return ErrorTipoIva(2, "Ya existe un tipo de IVA con ese código.");
                if (!insertar && existe == 0)
                    return ErrorTipoIva(1, "No se encontró el tipo de IVA.");

                int cuentaValida = connection.ExecuteScalar<int>(
                    @"SELECT COUNT(1)
                      FROM vCNTX_CUENTAS_LOCAL
                      WHERE COD_CUENTA = @Cuenta AND ACEPTA_MOVIMIENTOS = 1",
                    new { Cuenta = cuenta },
                    transaction);

                if (cuentaValida == 0)
                    return ErrorTipoIva(2, "La cuenta indicada no es válida o no acepta movimientos.");

                if (insertar)
                {
                    connection.Execute(
                        @"INSERT INTO SYS_IVA_TIPOS
                            (COD_IVA, DESCRIPCION, PORCENTAJE, COD_CUENTA)
                          VALUES
                            (@Codigo, @Descripcion, @Porcentaje, @Cuenta)",
                        new
                        {
                            Codigo = codigo,
                            Descripcion = descripcion,
                            Porcentaje = request.Porcentaje.GetValueOrDefault(),
                            Cuenta = cuenta
                        },
                        transaction);
                }
                else
                {
                    connection.Execute(
                        @"UPDATE SYS_IVA_TIPOS
                             SET DESCRIPCION = @Descripcion,
                                 PORCENTAJE = @Porcentaje,
                                 COD_CUENTA = @Cuenta
                           WHERE COD_IVA = @Codigo",
                        new
                        {
                            Codigo = codigo,
                            Descripcion = descripcion,
                            Porcentaje = request.Porcentaje.GetValueOrDefault(),
                            Cuenta = cuenta
                        },
                        transaction);
                }

                _security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    Modulo = vModulo,
                    Movimiento = insertar ? "Registra - WEB" : "Modifica - WEB",
                    DetalleMovimiento = $"Tipo de IVA: {codigo} -> {descripcion}; {request.Porcentaje.GetValueOrDefault()}%"
                });

                var tipoIva = connection.QueryFirstOrDefault<TipoIvaFactura>(
                    "dbo.spSys_IVA_TIPOS_Consulta",
                    new { TipoIVA = codigo },
                    transaction,
                    commandType: CommandType.StoredProcedure);

                if (tipoIva is null)
                    throw new InvalidOperationException("No se pudo consultar el tipo de IVA guardado.");

                CompletarDatosCuentaIva(connection, new[] { tipoIva }, transaction);
                transaction.Commit();
                return new ErrorDto<TipoIvaFactura>
                {
                    Code = 0,
                    Description = "Ok",
                    Result = tipoIva
                };
            }
            catch (Exception ex)
            {
                return ErrorTipoIva(-1, ex.Message);
            }
        }

        private static ErrorDto<TipoIvaFactura>? ValidarDatosTipoIva(
            string codigo,
            string descripcion,
            string cuenta,
            SysIvaTipoSaveRequest request)
        {
            if (string.IsNullOrWhiteSpace(codigo) || codigo.Length > 2)
                return ErrorTipoIva(2, "El código de IVA debe tener uno o dos caracteres.");
            if (string.IsNullOrWhiteSpace(descripcion))
                return ErrorTipoIva(2, "La descripción del tipo de IVA es obligatoria.");
            if (request.Porcentaje is null || request.Porcentaje < 0 || request.Porcentaje > 100)
                return ErrorTipoIva(2, "El porcentaje debe estar entre 0 y 100.");
            if (string.IsNullOrWhiteSpace(cuenta))
                return ErrorTipoIva(2, "La cuenta contable es obligatoria.");

            return null;
        }

        private static ErrorDto<TipoIvaFactura> ErrorTipoIva(int code, string description)
        {
            return new ErrorDto<TipoIvaFactura>
            {
                Code = code,
                Description = description
            };
        }

        private static void CompletarDatosCuentaIva(
            IDbConnection connection,
            IEnumerable<TipoIvaFactura> tipos,
            IDbTransaction? transaction = null)
        {
            var tiposConCuenta = tipos
                .Where(tipo => !string.IsNullOrWhiteSpace(tipo.Cod_Cuenta))
                .ToList();
            var cuentas = tiposConCuenta
                .Select(tipo => tipo.Cod_Cuenta.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (cuentas.Length == 0)
                return;

            var cuentasContables = connection.Query<CuentaTipoIvaData>(
                @"SELECT COD_CUENTA AS Cod_Cuenta,
                         COD_CUENTA_MASK AS Cod_Cuenta_Mask,
                         DESCRIPCION AS CTA_Descripcion
                  FROM vCNTX_CUENTAS_LOCAL
                  WHERE COD_CUENTA IN @Cuentas",
                new { Cuentas = cuentas },
                transaction)
                .GroupBy(cuenta => cuenta.Cod_Cuenta, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    grupo => grupo.Key,
                    grupo => grupo.First(),
                    StringComparer.OrdinalIgnoreCase);

            foreach (var tipo in tiposConCuenta.Where(tipo => cuentasContables.ContainsKey(tipo.Cod_Cuenta.Trim())))
            {
                var cuenta = cuentasContables[tipo.Cod_Cuenta.Trim()];

                tipo.Cod_Cuenta_Mask = cuenta.Cod_Cuenta_Mask;
                tipo.CTA_Descripcion = cuenta.CTA_Descripcion;
            }
        }

        private sealed class CuentaTipoIvaData
        {
            public string Cod_Cuenta { get; set; } = string.Empty;
            public string Cod_Cuenta_Mask { get; set; } = string.Empty;
            public string CTA_Descripcion { get; set; } = string.Empty;
        }
    }
}
