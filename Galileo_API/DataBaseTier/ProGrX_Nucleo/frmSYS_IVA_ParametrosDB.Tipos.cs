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
                result.Result = ConsultarTiposIva(connection)
                    .OrderBy(tipo => tipo.Cod_IVA).ToList();
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

                var anterior = ConsultarTiposIva(connection, codigo, transaction).FirstOrDefault();
                if (insertar && anterior is not null)
                    return ErrorTipoIva(2, "Ya existe un tipo de IVA con ese código.");
                if (!insertar && anterior is null)
                    return ErrorTipoIva(1, "No se encontró el tipo de IVA.");

                int cuentaValida = connection.ExecuteScalar<int>(
                    @"SELECT COUNT(1)
                      FROM vCNTX_CUENTAS_LOCAL
                      WHERE COD_CUENTA = @Cuenta AND ACEPTA_MOVIMIENTOS = 1",
                    new { Cuenta = cuenta },
                    transaction);
                if (cuentaValida == 0)
                    return ErrorTipoIva(2, "La cuenta indicada no es válida o no acepta movimientos.");

                var parametros = new DynamicParameters();
                parametros.Add("@CodIVA", codigo);
                parametros.Add("@Descripcion", descripcion);
                parametros.Add("@Activo", request.Activo ?? anterior?.Activo ?? 1);
                parametros.Add("@CodCuenta", cuenta);
                parametros.Add("@Porcentaje", request.Porcentaje.GetValueOrDefault());
                parametros.Add("@CodHacienda", (request.Cod_Hacienda ?? anterior?.Cod_Hacienda ?? codigo).Trim());
                parametros.Add("@Usuario", usuario);
                parametros.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

                var movimiento = connection.QueryFirstOrDefault<TipoIvaMovimiento>(
                    "dbo.spSys_IVA_Tipos_Add",
                    parametros,
                    transaction,
                    commandType: CommandType.StoredProcedure);
                var errorMovimiento = ValidarResultadoMovimiento(
                    parametros.Get<int>("@ReturnValue"),
                    movimiento,
                    insertar);
                if (errorMovimiento is not null)
                    return errorMovimiento;

                _security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    Modulo = vModulo,
                    Movimiento = insertar ? "Registra - WEB" : "Modifica - WEB",
                    DetalleMovimiento = $"Tipo de IVA: {codigo} -> {descripcion}; {request.Porcentaje.GetValueOrDefault()}%"
                });

                var tipoIva = ConsultarTiposIva(connection, codigo, transaction).FirstOrDefault();

                if (tipoIva is null)
                    throw new InvalidOperationException("No se pudo consultar el tipo de IVA guardado.");

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

        private static ErrorDto<TipoIvaFactura>? ValidarResultadoMovimiento(
            int resultado,
            TipoIvaMovimiento? movimiento,
            bool insertar)
        {
            if (resultado != 0 || movimiento is null || movimiento.Movimiento == "Error")
                return ErrorTipoIva(2, movimiento?.Mensaje ?? "No se pudo guardar el tipo de IVA.");
            if (insertar && movimiento.Movimiento != "Inserta")
                return ErrorTipoIva(2, "El tipo de IVA ya existe.");
            if (!insertar && movimiento.Movimiento != "Modifica")
                return ErrorTipoIva(1, "El tipo de IVA dejó de existir; recargue la tabla.");

            return null;
        }

        private static List<TipoIvaFactura> ConsultarTiposIva(
            SqlConnection connection,
            string? codigo = null,
            IDbTransaction? transaction = null)
        {
            var filas = connection.Query<TipoIvaConsultaRow>(
                "dbo.spSys_IVA_TIPOS_Consulta",
                new { TipoIVA = codigo },
                transaction,
                commandType: CommandType.StoredProcedure);

            return filas.GroupBy(fila => fila.COD_IVA, StringComparer.OrdinalIgnoreCase)
                .Select(grupo => grupo.First())
                .Select(fila =>
                {
                    if (string.IsNullOrWhiteSpace(fila.COD_HACIENDA) || fila.ACTIVO is not (0 or 1))
                        throw new InvalidOperationException(
                            $"El procedimiento no devolvió Código Hacienda o Activo válido para el tipo de IVA {fila.COD_IVA}.");
                    return new TipoIvaFactura
                    {
                        Cod_IVA = fila.COD_IVA,
                        Descripcion = fila.DESCRIPCION,
                        Porcentaje = fila.PORCENTAJE,
                        Cod_Cuenta = fila.COD_CUENTA,
                        Cod_Hacienda = fila.COD_HACIENDA,
                        Activo = fila.ACTIVO.Value,
                        Cod_Cuenta_Mask = fila.COD_CUENTA_MASK ?? string.Empty,
                        CTA_Descripcion = fila.CTA_DESCRIPCION ?? string.Empty
                    };
                }).ToList();
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

    }
}
