using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndSubCuentasDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 18; // Modulo de Fondo de Inversion
        private readonly MSecurityMainDb _Security_MainDB;

        private const string SqlSubCuentas = @"
                    SELECT
                        B.IdX AS idx,
                        B.cedula,
                        B.Nombre AS nombre,
                        B.parentesco,
                        B.fechaNac AS fechanac,
                        B.cuota,
                        B.APORTES AS aportes,
                        B.RENDIMIENTO AS rendimiento,
                        B.direccion,
                        B.notas,
                        B.telefono1,
                        B.telefono2,
                        B.email,
                        B.apto_postal,
                        B.cod_operadora,
                        B.cod_plan,
                        B.cod_contrato,
                        B.cod_beneficiario,
                        B.estado,
                        ISNULL(P.Descripcion, '') AS Parentesco_Desc
                    FROM dbo.FND_SubCUENTAS B
                    LEFT JOIN dbo.SYS_PARENTESCOS P
                        ON B.Parentesco = P.cod_Parentesco
                    WHERE B.cod_Operadora = @Operadora
                      AND B.cod_Plan = @Plan
                      AND B.cod_Contrato = @Contrato
                    ORDER BY B.IdX;";

        private const string SqlParentescos = @"
                    SELECT
                        RTRIM(cod_Parentesco) AS item,
                        RTRIM(Descripcion) AS descripcion
                    FROM dbo.sys_Parentescos
                    WHERE activo = 1
                    ORDER BY Descripcion;";

        private const string SqlExisteBeneficiario = @"
                    SELECT ISNULL(COUNT(1), 0)
                    FROM dbo.FND_SubCUENTAS
                    WHERE cedula = @Cedula
                      AND IDX <> @Idx
                      AND cod_Operadora = @Operadora
                      AND cod_Plan = @Plan
                      AND cod_Contrato = @Contrato;";

        private const string SqlSiguienteIdx = @"
                    SELECT ISNULL(MAX(IDX), 0) + 1
                    FROM dbo.FND_SubCUENTAS
                    WHERE cod_operadora = @Operadora
                      AND cod_plan = @Plan
                      AND cod_contrato = @Contrato;";

        private const string SqlInsertSubCuenta = @"
                    INSERT INTO dbo.FND_SubCUENTAS
                    (
                        IdX,
                        cedula,
                        Nombre,
                        parentesco,
                        fechaNac,
                        cuota,
                        APORTES,
                        RENDIMIENTO,
                        direccion,
                        notas,
                        telefono1,
                        telefono2,
                        email,
                        apto_postal,
                        cod_operadora,
                        cod_plan,
                        cod_contrato,
                        cod_beneficiario,
                        estado
                    )
                    VALUES
                    (
                        @IdX,
                        @Cedula,
                        @Nombre,
                        @Parentesco,
                        @FechaNac,
                        @Cuota,
                        @Aportes,
                        @Rendimiento,
                        @Direccion,
                        @Notas,
                        @Telefono1,
                        @Telefono2,
                        @Email,
                        @AptoPostal,
                        @CodOperadora,
                        @CodPlan,
                        @CodContrato,
                        @CodBeneficiario,
                        @Estado
                    );";

        private const string SqlUpdateSubCuenta = @"
                    UPDATE dbo.FND_SubCUENTAS
                    SET Nombre = @Nombre,
                        CEDULA = @Cedula,
                        parentesco = @Parentesco,
                        notas = @Notas,
                        direccion = @Direccion,
                        apto_postal = @AptoPostal,
                        email = @Email,
                        telefono1 = @Telefono1,
                        telefono2 = @Telefono2,
                        fechaNac = @FechaNac,
                        cuota = @Cuota
                    WHERE IdX = @IdX;";

        private const string SqlSubCuentaPorIdx = @"
                    SELECT
                        IdX AS idx,
                        cedula,
                        Nombre AS nombre,
                        parentesco,
                        fechaNac AS fechanac,
                        cuota,
                        APORTES AS aportes,
                        RENDIMIENTO AS rendimiento,
                        direccion,
                        notas,
                        telefono1,
                        telefono2,
                        email,
                        apto_postal,
                        cod_operadora,
                        cod_plan,
                        cod_contrato,
                        cod_beneficiario,
                        estado
                    FROM dbo.FND_SubCUENTAS
                    WHERE IDX = @Idx;";

        private const string SqlDeleteSubCuenta = @"
                    DELETE FROM dbo.FND_SubCUENTAS
                    WHERE IDX = @Idx;";

        private const string SqlCedulaConsecutivo = @"
                    SELECT CONVERT(varchar(20), ISNULL(COUNT(1), 0) + 1)
                    FROM dbo.FND_SubCUENTAS
                    WHERE cod_plan = @Plan
                      AND cod_contrato = @Contrato
                      AND cod_operadora = @Operadora;";

        public FrmFndSubCuentasDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Método que obtiene la lista de SubCuentas de un contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="operadora"></param>
        /// <param name="plan"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        public ErrorDto<List<FndSubCuentasData>> FND_SubCuentas_Lista_Obtener(int CodEmpresa, int operadora, string plan, long contrato)
        {
            return DbHelper.ExecuteListQuery<FndSubCuentasData>(
                new PortalDB(_config),
                CodEmpresa,
                SqlSubCuentas,
                new
                {
                    Operadora = operadora,
                    Plan = NormalizarTexto(plan),
                    Contrato = contrato
                });
        }


        /// <summary>
        /// Método para obtener la lista de parentescos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FND_SubCuentas_Parentescos_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                SqlParentescos);
        }

        /// <summary>
        /// Me
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto FND_SubCuentas_Guardar(int CodEmpresa, string usuario, FndSubCuentasData data)
        {
            if (data is null)
            {
                return DbHelper.ErrorResponse("Los datos de la subcuenta son requeridos.", -2);
            }

            var valida = fxValida(CodEmpresa, data);
            if (valida.Code != 0)
            {
                return DbHelper.ErrorResponse(valida.Description ?? "La subcuenta no es válida.", valida.Code.GetValueOrDefault(-1));
            }

            return data.isNew
                ? FNDSubCuentas_Insertar(CodEmpresa, usuario, data)
                : FNDSubCuentas_Actualizar(CodEmpresa, usuario, data);
        }

        /// <summary>
        /// Método para validar la informacion de la SubCuenta
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private ErrorDto<bool> fxValida(int CodEmpresa, FndSubCuentasData data)
        {
            var response = DbHelper.CreateOkResponse(true, string.Empty);

            if (data is null)
            {
                return DbHelper.CreateErrorResponse("Los datos de la subcuenta son requeridos.", -2, false);
            }

            var existe = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlExisteBeneficiario,
                0,
                new
                {
                    Cedula = NormalizarTexto(data.cedula),
                    Idx = data.idx,
                    Operadora = data.cod_operadora,
                    Plan = NormalizarTexto(data.cod_plan),
                    Contrato = data.cod_contrato
                });

            if (existe.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    existe.Description ?? "Error al validar subcuenta.",
                    existe.Code.GetValueOrDefault(-1),
                    false);
            }

            var mensajes = new List<string>();
            if (existe.Result > 0)
            {
                mensajes.Add("Ya existe un beneficiario registrado con el mismo número de identificación.");
            }

            AgregarValidacionesRequeridas(data, mensajes);
            if (mensajes.Count == 0)
            {
                return response;
            }

            response.Code = -1;
            response.Description = " - " + string.Join(" - ", mensajes);
            response.Result = false;
            return response;
        }

        /// <summary>
        /// Método para insertar una SubCuenta de un contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private ErrorDto FNDSubCuentas_Insertar(int CodEmpresa, string usuario, FndSubCuentasData data)
        {
            var idxResult = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlSiguienteIdx,
                1,
                new
                {
                    Operadora = data.cod_operadora,
                    Plan = NormalizarTexto(data.cod_plan),
                    Contrato = data.cod_contrato
                });

            if (idxResult.Code != 0)
            {
                return DbHelper.ErrorResponse(idxResult.Description ?? "Error al obtener consecutivo de subcuenta.", idxResult.Code.GetValueOrDefault(-1));
            }

            data.idx = idxResult.Result;

            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlInsertSubCuenta,
                CrearParametrosSubCuenta(data, true));

            if (result.Code == 0)
            {
                RegistrarBitacora(CodEmpresa, usuario, data, "Registra - WEB");
            }

            return result;
        }

        /// <summary>
        /// Método para actualizar una SubCuenta de un contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private ErrorDto FNDSubCuentas_Actualizar(int CodEmpresa, string usuario, FndSubCuentasData data)
        {
            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlUpdateSubCuenta,
                CrearParametrosSubCuenta(data, false));

            if (result.Code == 0)
            {
                RegistrarBitacora(CodEmpresa, usuario, data, "Modifica - WEB");
            }

            return result;
        }

        /// <summary>
        /// Método para eliminar una SubCuenta de un contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="consec"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto FNDSubCuentas_Borrar(int CodEmpresa, int consec, string usuario)
        {
            var existe = DbHelper.ExecuteSingleQuery<FndSubCuentasData>(
                new PortalDB(_config),
                CodEmpresa,
                SqlSubCuentaPorIdx,
                default,
                new { Idx = consec });

            if (existe.Code != 0)
            {
                return DbHelper.ErrorResponse(existe.Description ?? "Error al consultar subcuenta.", existe.Code.GetValueOrDefault(-1));
            }

            if (existe.Result is null)
            {
                return DbHelper.ErrorResponse("La subcuenta no existe.", -2);
            }

            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlDeleteSubCuenta,
                new { Idx = consec });

            if (result.Code == 0)
            {
                RegistrarBitacora(CodEmpresa, usuario, existe.Result, "Elimina - WEB");
            }

            return result;
        }

        /// <summary>
        /// Método para obtener el consecutivo de un beneficiario por cedula
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="plan"></param>
        /// <param name="contrato"></param>
        /// <param name="operadora"></param>
        /// <returns></returns>
        public ErrorDto<string> FNDDSubCuentas_Cedula_Obtener(int CodEmpresa, string plan, long contrato, int operadora)
        {
            var result = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlCedulaConsecutivo,
                string.Empty,
                new
                {
                    Plan = NormalizarTexto(plan),
                    Contrato = contrato,
                    Operadora = operadora
                });

            return new ErrorDto<string>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? string.Empty
            };
        }

        private static void AgregarValidacionesRequeridas(FndSubCuentasData data, List<string> mensajes)
        {
            if (string.IsNullOrWhiteSpace(data.parentesco))
            {
                mensajes.Add("No se ha seleccionado ningún parentesco.");
            }

            if (string.IsNullOrWhiteSpace(data.nombre))
            {
                mensajes.Add("Nombre del beneficiario no es válido.");
            }

            if (string.IsNullOrWhiteSpace(data.apellido1))
            {
                mensajes.Add("Primer apellido del beneficiario no es válido.");
            }

            if (string.IsNullOrWhiteSpace(data.apellido2))
            {
                mensajes.Add("Segundo apellido del beneficiario no es válido.");
            }
        }

        private static object CrearParametrosSubCuenta(FndSubCuentasData data, bool esNuevo)
        {
            return new
            {
                IdX = data.idx,
                Cedula = NormalizarTexto(data.cedula),
                Nombre = CrearNombreCompleto(data),
                Parentesco = NormalizarTexto(data.parentesco),
                FechaNac = MProGrXAuxiliarDB.validaFechaGlobal(data.fechanac, esNuevo ? "yyyyMMdd" : "yyyy-MM-dd"),
                Cuota = esNuevo ? 0 : data.cuota,
                Aportes = 0,
                Rendimiento = 0,
                Direccion = NormalizarTexto(data.direccion),
                Notas = NormalizarTexto(data.notas),
                Telefono1 = NormalizarTexto(data.telefono1),
                Telefono2 = NormalizarTexto(data.telefono2),
                Email = NormalizarTexto(data.email),
                AptoPostal = NormalizarTexto(data.apto_postal),
                CodOperadora = data.cod_operadora,
                CodPlan = NormalizarTexto(data.cod_plan),
                CodContrato = data.cod_contrato,
                CodBeneficiario = 0,
                Estado = "A"
            };
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, FndSubCuentasData data, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario),
                DetalleMovimiento = $"Sub-Cuenta de Plan: Op. {data.cod_operadora}..Pln: {NormalizarTexto(data.cod_plan)}..Cnt:{data.cod_contrato}..Id:{data.idx}..Ced.{NormalizarTexto(data.cedula)}, Mnt.{data.cuota}",
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private static string CrearNombreCompleto(FndSubCuentasData data)
        {
            return $"{NormalizarTexto(data.nombre)} {NormalizarTexto(data.apellido1)} {NormalizarTexto(data.apellido2)}"
                .Trim()
                .ToUpperInvariant();
        }

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();

    }
}