using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndBeneficiariosContratosDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 18; // Modulo de Fondo de Inversion
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmFndBeneficiariosContratosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Método para obtener la lista de beneficiarios de un contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="operadora"></param>
        /// <param name="plan"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        public ErrorDto<List<FndBeneficiariosContratosData>> FND_Beneficiarios_Contratos_Lista_Obtener(int CodEmpresa, string cedula, int operadora, string plan, long contrato)
        {
            const string query = @"
                    SELECT
                        B.*,
                        ISNULL(P.Descripcion, '') AS Parentesco_Desc
                    FROM dbo.FND_CONTRATOS_BENEFICIARIOS B
                    LEFT JOIN dbo.SYS_PARENTESCOS P
                        ON B.Parentesco = P.cod_Parentesco
                    WHERE B.Cedula = @cedula
                      AND B.cod_Operadora = @operadora
                      AND B.cod_Plan = @plan
                      AND B.cod_Contrato = @contrato;";

            return DbHelper.ExecuteListQuery<FndBeneficiariosContratosData>(
                new PortalDB(_config),
                CodEmpresa,
                query,
                CrearParametrosContrato(cedula, operadora, plan, contrato));
        }


        /// <summary>
        /// Método para obtener la lista de parentescos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FND_Beneficiarios_Contratos_Parentescos_Obtener(int CodEmpresa)
        {
            const string query = @"
                    SELECT
                        RTRIM(cod_Parentesco) AS item,
                        RTRIM(Descripcion) AS descripcion
                    FROM dbo.sys_Parentescos
                    WHERE activo = 1
                    ORDER BY Descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(new PortalDB(_config), CodEmpresa, query);
        }

        /// <summary>
        /// Método para guardar o actualizar un beneficiario de un contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto FND_Beneficiarios_Contratos_Guardar(int CodEmpresa, string usuario, FndBeneficiariosContratosData data)
        {
            if (data is null)
            {
                return DbHelper.ErrorResponse("Los datos del beneficiario son requeridos.", -2);
            }

            var valida = FxValida(CodEmpresa, data);
            if (valida.Code != 0)
            {
                return DbHelper.ErrorResponse(valida.Description ?? string.Empty, valida.Code ?? -1);
            }

            var result = data.isNew
                ? FNDBeneficiarios_Contratos_Insertar(CodEmpresa, usuario, data)
                : FNDBeneficiarios_Contratos_Actualizar(CodEmpresa, usuario, data);

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                CrearDetalleBeneficiario(data),
                data.isNew ? "Registra - WEB" : "Modifica - WEB");

            return result;
        }

        /// <summary>
        /// Método para validar la informacion del beneficiario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private ErrorDto<bool> FxValida(int CodEmpresa, FndBeneficiariosContratosData data)
        {
            var errores = new List<string>();

            var existeResult = ExisteBeneficiario(CodEmpresa, data);
            if (existeResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse(existeResult.Description ?? "Error al validar beneficiario.", existeResult.Code ?? -1, false);
            }

            if (existeResult.Result > 0)
            {
                errores.Add("Ya existe un beneficiario registrado con el mismo número de identificación.");
            }

            AgregarErroresDatosBeneficiario(data, errores);
            AgregarErrorPorcentaje(CodEmpresa, data, errores);

            return errores.Count == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse(string.Join(" ", errores), -1, false);
        }

        /// <summary>
        /// Método para insertar un nuevo beneficiario de un contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private ErrorDto FNDBeneficiarios_Contratos_Insertar(int CodEmpresa, string usuario, FndBeneficiariosContratosData data)
        {
            const string query = @"
                    INSERT INTO dbo.FND_CONTRATOS_BENEFICIARIOS
                    (
                        cedula,
                        cedulaBN,
                        nombre,
                        parentesco,
                        fechaNac,
                        porcentaje,
                        direccion,
                        notas,
                        telefono1,
                        telefono2,
                        email,
                        apto_postal,
                        cod_operadora,
                        cod_plan,
                        cod_contrato
                    )
                    VALUES
                    (
                        @cedula,
                        @cedulaBN,
                        @nombre,
                        @parentesco,
                        @fechaNac,
                        @porcentaje,
                        @direccion,
                        @notas,
                        @telefono1,
                        @telefono2,
                        @email,
                        @apto_postal,
                        @cod_operadora,
                        @cod_plan,
                        @cod_contrato
                    );";

            return DbHelper.ExecuteNonQuery(new PortalDB(_config), CodEmpresa, query, CrearParametrosBeneficiario(data));
        }

        /// <summary>
        /// Método para actualizar un beneficiario de un contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private ErrorDto FNDBeneficiarios_Contratos_Actualizar(int CodEmpresa, string usuario, FndBeneficiariosContratosData data)
        {
            const string query = @"
                    UPDATE dbo.FND_CONTRATOS_BENEFICIARIOS
                    SET
                        nombre = @nombre,
                        cedulaBN = @cedulaBN,
                        parentesco = @parentesco,
                        notas = @notas,
                        direccion = @direccion,
                        apto_postal = @apto_postal,
                        email = @email,
                        telefono1 = @telefono1,
                        telefono2 = @telefono2,
                        fechaNac = @fechaNac,
                        porcentaje = @porcentaje
                    WHERE consec = @consec;";

            return DbHelper.ExecuteNonQuery(new PortalDB(_config), CodEmpresa, query, CrearParametrosBeneficiario(data));
        }

        /// <summary>
        /// Método para eliminar un beneficiario de un contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="consec"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto FNDBeneficiarios_Contratos_Borrar(int CodEmpresa, int consec, string usuario)
        {
            var beneficiario = ObtenerBeneficiarioPorConsecutivo(CodEmpresa, consec);
            if (beneficiario.Code != 0)
            {
                return DbHelper.ErrorResponse(beneficiario.Description ?? "Error al consultar beneficiario.");
            }

            const string query = @"
                    DELETE FROM dbo.FND_CONTRATOS_BENEFICIARIOS
                    WHERE consec = @consec;";
            var result = DbHelper.ExecuteNonQuery(new PortalDB(_config), CodEmpresa, query, new { consec });
            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                CrearDetalleEliminacionBeneficiario(beneficiario.Result, consec),
                "Elimina - WEB");

            return result;
        }

        /// <summary>
        /// Método para obtener el consecutivo del beneficiario por cedula
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="plan"></param>
        /// <param name="contrato"></param>
        /// <param name="operadora"></param>
        /// <returns></returns>
        public ErrorDto<string> FNDBene_Cnt_CedulaBN_Obtener(int CodEmpresa, string cedula, string plan, long contrato, int operadora)
        {
            const string query = @"
                    SELECT CAST(ISNULL(COUNT(1), 0) + 1 AS varchar(20)) AS Consec
                    FROM dbo.FND_CONTRATOS_BENEFICIARIOS
                    WHERE cedula = @cedula
                      AND cod_plan = @plan
                      AND cod_contrato = @contrato
                      AND cod_operadora = @operadora;";

            var result = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                query,
                string.Empty,
                CrearParametrosContrato(cedula, operadora, plan, contrato));

            return new ErrorDto<string>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? string.Empty
            };
        }

        private ErrorDto<int> ExisteBeneficiario(int codEmpresa, FndBeneficiariosContratosData data)
        {
            const string query = @"
                    SELECT ISNULL(COUNT(1), 0) AS Existe
                    FROM dbo.FND_CONTRATOS_BENEFICIARIOS
                    WHERE cedula = @cedula
                      AND cedulaBN = @cedulaBN
                      AND consec <> @consec
                      AND cod_Operadora = @operadora
                      AND cod_Plan = @plan
                      AND cod_Contrato = @contrato;";

            return DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                codEmpresa,
                query,
                0,
                new
                {
                    cedula = NormalizarTexto(data.cedula),
                    cedulaBN = NormalizarTexto(data.cedulabn),
                    consec = data.consec,
                    operadora = data.cod_operadora,
                    plan = NormalizarTexto(data.cod_plan),
                    contrato = data.cod_contrato
                });
        }

        private void AgregarErrorPorcentaje(int codEmpresa, FndBeneficiariosContratosData data, List<string> errores)
        {
            if (data.porcentaje <= 0)
            {
                errores.Add("El porcentaje no es válido.");
                return;
            }

            var porcentaje = ObtenerPorcentajeActual(codEmpresa, data);
            if (porcentaje.Code != 0)
            {
                errores.Add(porcentaje.Description ?? "Error al validar porcentaje.");
                return;
            }

            if (data.porcentaje + porcentaje.Result > 100)
            {
                errores.Add("El porcentaje sobrepasa el total del 100% de los beneficiarios.");
            }
        }

        private ErrorDto<decimal> ObtenerPorcentajeActual(int codEmpresa, FndBeneficiariosContratosData data)
        {
            const string query = @"
                    SELECT ISNULL(SUM(porcentaje), 0) AS Porcentaje
                    FROM dbo.FND_CONTRATOS_BENEFICIARIOS
                    WHERE cedula = @cedula
                      AND consec <> @consec
                      AND cod_Operadora = @operadora
                      AND cod_Plan = @plan
                      AND cod_Contrato = @contrato;";

            return DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                codEmpresa,
                query,
                0m,
                new
                {
                    cedula = NormalizarTexto(data.cedula),
                    consec = data.consec,
                    operadora = data.cod_operadora,
                    plan = NormalizarTexto(data.cod_plan),
                    contrato = data.cod_contrato
                });
        }

        private static void AgregarErroresDatosBeneficiario(FndBeneficiariosContratosData data, List<string> errores)
        {
            if (string.IsNullOrWhiteSpace(data.parentesco))
            {
                errores.Add("No se ha seleccionado ningún parentesco.");
            }

            if (string.IsNullOrWhiteSpace(data.nombre) ||
                string.IsNullOrWhiteSpace(data.apellido1) ||
                string.IsNullOrWhiteSpace(data.apellido2))
            {
                errores.Add("Nombre del beneficiario no es válido.");
            }
        }

        private ErrorDto<FndBeneficiariosContratosData?> ObtenerBeneficiarioPorConsecutivo(int codEmpresa, int consec)
        {
            const string query = @"
                    SELECT
                        consec,
                        cedula,
                        cedulaBN,
                        nombre,
                        parentesco,
                        fechaNac,
                        porcentaje,
                        direccion,
                        notas,
                        telefono1,
                        telefono2,
                        email,
                        apto_postal,
                        cod_operadora,
                        cod_plan,
                        cod_contrato
                    FROM dbo.FND_CONTRATOS_BENEFICIARIOS
                    WHERE consec = @consec;";
            return DbHelper.ExecuteSingleQuery<FndBeneficiariosContratosData>(new PortalDB(_config), codEmpresa, query, null, new { consec });
        }

        private static object CrearParametrosBeneficiario(FndBeneficiariosContratosData data)
        {
            return new
            {
                cedula = NormalizarTexto(data.cedula),
                cedulaBN = NormalizarTexto(data.cedulabn),
                nombre = CrearNombreCompleto(data),
                parentesco = NormalizarTexto(data.parentesco),
                fechaNac = MProGrXAuxiliarDB.validaFechaGlobal(data.fechanac, "yyyy-MM-dd"),
                porcentaje = data.porcentaje,
                direccion = data.direccion,
                notas = data.notas,
                telefono1 = data.telefono1,
                telefono2 = data.telefono2,
                email = data.email,
                apto_postal = data.apto_postal,
                cod_operadora = data.cod_operadora,
                cod_plan = NormalizarTexto(data.cod_plan),
                cod_contrato = data.cod_contrato,
                consec = data.consec
            };
        }

        private static object CrearParametrosContrato(string cedula, int operadora, string plan, long contrato)
        {
            return new
            {
                cedula = NormalizarTexto(cedula),
                operadora,
                plan = NormalizarTexto(plan),
                contrato
            };
        }

        private static string CrearNombreCompleto(FndBeneficiariosContratosData data)
        {
            return $"{NormalizarTexto(data.nombre).ToUpper()} {NormalizarTexto(data.apellido1).ToUpper()} {NormalizarTexto(data.apellido2).ToUpper()}".Trim();
        }

        private static string CrearDetalleBeneficiario(FndBeneficiariosContratosData data)
        {
            return $"Beneficiario de Plan: Op. {data.cod_operadora}..Pln:{NormalizarTexto(data.cod_plan)}....Cnt:{data.cod_contrato}.Id: {data.consec}";
        }

        private static string CrearDetalleEliminacionBeneficiario(FndBeneficiariosContratosData? data, int consec)
        {
            return data is null
                ? $"Beneficiario de Plan: Datos no encontrados para consec: {consec}"
                : CrearDetalleBeneficiario(data);
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalleMovimiento, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalleMovimiento,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}
