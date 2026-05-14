using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public partial class FrmFndContratosDB
    {
        private const string SpSubCuentasMaestroUpdate = "spFnd_SubCuentas_Maestro_Update";

        private const string SqlContratoSubCuentas = @"
                    SELECT
                        idx,
                        cedula,
                        nombre,
                        cuota,
                        0 AS acumulado
                    FROM dbo.fnd_subCuentas
                    WHERE cod_operadora = @Operadora
                      AND cod_plan = @Plan
                      AND cod_contrato = @Contrato;";

        private const string SqlSubCuentaSiguiente = @"
                    SELECT ISNULL(COUNT(1), 0) + 1
                    FROM dbo.fnd_subCuentas
                    WHERE cod_operadora = @Operadora
                      AND cod_plan = @Plan
                      AND cod_contrato = @Contrato;";

        private const string SqlInsertSubCuentaContrato = @"
                    INSERT INTO dbo.fnd_subCuentas
                    (
                        cod_operadora,
                        cod_plan,
                        cod_contrato,
                        idX,
                        cedula,
                        nombre,
                        cuota,
                        estado,
                        aportes,
                        rendimiento,
                        telefono1,
                        telefono2,
                        notas,
                        email,
                        apto_postal,
                        direccion,
                        parentesco,
                        cod_beneficiario
                    )
                    VALUES
                    (
                        @CodOperadora,
                        @CodPlan,
                        @CodContrato,
                        @Idx,
                        @Cedula,
                        @Nombre,
                        @Cuota,
                        @Estado,
                        @Aportes,
                        @Rendimiento,
                        @Telefono1,
                        @Telefono2,
                        @Notas,
                        @Email,
                        @AptoPostal,
                        @Direccion,
                        @Parentesco,
                        @CodBeneficiario
                    );";

        private const string SqlUpdateSubCuentaContrato = @"
                    UPDATE dbo.fnd_subCuentas
                    SET cedula = @Cedula,
                        nombre = @Nombre,
                        cuota = @Cuota
                    WHERE idx = @Idx
                      AND cod_operadora = @CodOperadora
                      AND cod_plan = @CodPlan
                      AND cod_contrato = @CodContrato;";

        #region SubCuentas

        /// <summary>
        /// Obtiene las subcuentas asociadas a un contrato y agrega la siguiente subcuenta sugerida.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="pOperadora">Código de operadora.</param>
        /// <param name="pPlan">Código del plan.</param>
        /// <param name="pContrato">Número de contrato.</param>
        /// <param name="cedula">Cédula base del titular.</param>
        /// <returns>Listado de subcuentas del contrato.</returns>
        public ErrorDto<List<FndContratoSubCuentasData>> Fnd_Contratos_SubCuentas_Obtener(int CodEmpresa, int pOperadora, string pPlan, long pContrato, string cedula)
        {
            var response = DbHelper.ExecuteListQuery<FndContratoSubCuentasData>(
                CreatePortalDb(),
                CodEmpresa,
                SqlContratoSubCuentas,
                CrearParametrosSubCuentaBase(pOperadora, pPlan, pContrato));

            if (response.Code != 0)
            {
                return response;
            }

            response.Result ??= new List<FndContratoSubCuentasData>();

            var consec = fxSubCuentaContrato(CodEmpresa, pOperadora, pPlan, pContrato).Result;
            response.Result.Add(new FndContratoSubCuentasData
            {
                idx = 0,
                cedula = $"{NormalizarTexto(cedula)}-{consec:00}",
                nombre = "0",
                cuota = 0,
                cod_operadora = pOperadora,
                cod_contrato = pContrato,
                isNew = true
            });

            return response;
        }

        /// <summary>
        /// Obtiene el siguiente número disponible para una subcuenta de contrato.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="pOperadora">Código de operadora.</param>
        /// <param name="pPlan">Código del plan.</param>
        /// <param name="pContrato">Número de contrato.</param>
        /// <returns>Siguiente consecutivo de subcuenta.</returns>
        public ErrorDto<int> fxSubCuentaContrato(int CodEmpresa, int pOperadora, string pPlan, long pContrato)
        {
            return DbHelper.ExecuteSingleQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlSubCuentaSiguiente,
                0,
                CrearParametrosSubCuentaBase(pOperadora, pPlan, pContrato));
        }

        /// <summary>
        /// Guarda una subcuenta del contrato y actualiza la cuota del contrato maestro.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que realiza la operación.</param>
        /// <param name="subCuenta">Datos de la subcuenta.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Fnd_Contratos_SubCuentas_Guardar(int CodEmpresa, string usuario, FndContratoSubCuentasData subCuenta)
        {
            if (subCuenta is null)
            {
                return DbHelper.ErrorResponse("Los datos de la subcuenta son requeridos.", -2);
            }

            var response = subCuenta.isNew
                ? Fnd_Contratos_SubCuentas_Insertar(CodEmpresa, usuario, subCuenta)
                : Fnd_Contratos_SubCuentas_Actualizar(CodEmpresa, usuario, subCuenta);

            return response.Code == 0
                ? sbActualizaCuotaContrato(CodEmpresa, usuario, subCuenta)
                : response;
        }

        /// <summary>
        /// Inserta una subcuenta nueva para el contrato.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que registra la subcuenta.</param>
        /// <param name="subCuenta">Datos de la subcuenta.</param>
        /// <returns>Resultado de la inserción.</returns>
        private ErrorDto Fnd_Contratos_SubCuentas_Insertar(int CodEmpresa, string usuario, FndContratoSubCuentasData subCuenta)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlInsertSubCuentaContrato,
                CrearParametrosSubCuenta(subCuenta));

            if (result.Code == 0)
            {
                RegistrarBitacoraSubCuenta(CodEmpresa, usuario, subCuenta, "Registra - WEB");
            }

            return result;
        }

        /// <summary>
        /// Actualiza una subcuenta existente del contrato.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que modifica la subcuenta.</param>
        /// <param name="subCuenta">Datos de la subcuenta.</param>
        /// <returns>Resultado de la actualización.</returns>
        private ErrorDto Fnd_Contratos_SubCuentas_Actualizar(int CodEmpresa, string usuario, FndContratoSubCuentasData subCuenta)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlUpdateSubCuentaContrato,
                CrearParametrosSubCuenta(subCuenta));

            if (result.Code == 0)
            {
                var plan = NormalizarTexto(subCuenta.cod_plan);
                sbGuardaCambios(CodEmpresa, subCuenta.cod_operadora, plan, subCuenta.cod_contrato, usuario, 04, $"Modifica a SubCuenta: {NormalizarTexto(subCuenta.cedula)}");
                RegistrarBitacoraSubCuenta(CodEmpresa, usuario, subCuenta, "Modifica - WEB");
            }

            return result;
        }

        /// <summary>
        /// Actualiza la cuota del contrato maestro con base en sus subcuentas.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que realiza la actualización.</param>
        /// <param name="subCuenta">Subcuenta asociada al contrato.</param>
        /// <returns>Resultado de la actualización.</returns>
        private ErrorDto sbActualizaCuotaContrato(int CodEmpresa, string usuario, FndContratoSubCuentasData subCuenta)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    SpSubCuentasMaestroUpdate,
                    new
                    {
                        Operadora = subCuenta.cod_operadora,
                        Plan = NormalizarTexto(subCuenta.cod_plan),
                        Contrato = subCuenta.cod_contrato,
                        Usuario = NormalizarTexto(usuario)
                    },
                    commandType: System.Data.CommandType.StoredProcedure);

                return true;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse()
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar cuota del contrato.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Crea parámetros base para consultas de subcuentas por contrato.
        /// </summary>
        /// <param name="operadora">Código de operadora.</param>
        /// <param name="plan">Código del plan.</param>
        /// <param name="contrato">Número de contrato.</param>
        /// <returns>Parámetros seguros para Dapper.</returns>
        private static object CrearParametrosSubCuentaBase(int operadora, string plan, long contrato)
        {
            return new
            {
                Operadora = operadora,
                Plan = NormalizarTexto(plan),
                Contrato = contrato
            };
        }

        /// <summary>
        /// Crea parámetros para insertar o actualizar una subcuenta del contrato.
        /// </summary>
        /// <param name="subCuenta">Datos de la subcuenta.</param>
        /// <returns>Parámetros seguros para Dapper.</returns>
        private static object CrearParametrosSubCuenta(FndContratoSubCuentasData subCuenta)
        {
            return new
            {
                CodOperadora = subCuenta.cod_operadora,
                CodPlan = NormalizarTexto(subCuenta.cod_plan),
                CodContrato = subCuenta.cod_contrato,
                Idx = subCuenta.idx,
                Cedula = NormalizarTexto(subCuenta.cedula),
                Nombre = NormalizarTexto(subCuenta.nombre),
                Cuota = subCuenta.cuota,
                Estado = "A",
                Aportes = 0,
                Rendimiento = 0,
                Telefono1 = string.Empty,
                Telefono2 = string.Empty,
                Notas = string.Empty,
                Email = string.Empty,
                AptoPostal = string.Empty,
                Direccion = string.Empty,
                Parentesco = string.Empty,
                CodBeneficiario = 0
            };
        }

        /// <summary>
        /// Registra en bitácora la inserción o modificación de una subcuenta.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que realiza la operación.</param>
        /// <param name="subCuenta">Datos de la subcuenta.</param>
        /// <param name="movimiento">Tipo de movimiento registrado.</param>
        private void RegistrarBitacoraSubCuenta(int codEmpresa, string usuario, FndContratoSubCuentasData subCuenta, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario),
                DetalleMovimiento = $"SubCuenta: {NormalizarTexto(subCuenta.cedula)} Plan: {NormalizarTexto(subCuenta.cod_plan)} Contrato: {subCuenta.cod_contrato}",
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        #endregion
    }
}