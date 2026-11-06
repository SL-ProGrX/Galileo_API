using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public partial class FrmFndContratosDB
    {
        private const string SpContratoDestinosList = "spFnd_Contrato_Destinos_List";
        private const string SpContratoDestinosAdd = "spFnd_Contrato_Destinos_Add";

        private const string SqlExisteDestinoAhorro = @"
                    SELECT COUNT(1)
                    FROM dbo.FND_CONTRATOS_DESTINOS_AHORRO
                    WHERE ID_REGISTRO = @IdRegistro
                      AND COD_PLAN = @CodPlan
                      AND COD_CONTRATO = @CodContrato;";

        private const string SqlInsertDestinoAhorro = @"
                    INSERT INTO dbo.FND_CONTRATOS_DESTINOS_AHORRO
                    (
                        ID_DESTINO,
                        COD_PLAN,
                        COD_CONTRATO,
                        OBSERVACIONES,
                        FEC_REGISTRO,
                        USU_REGISTRO
                    )
                    VALUES
                    (
                        @IdDestino,
                        @CodPlan,
                        @CodContrato,
                        @Observaciones,
                        dbo.MyGetdate(),
                        @Usuario
                    );";

        private const string SqlUpdateDestinoAhorro = @"
                    UPDATE dbo.FND_CONTRATOS_DESTINOS_AHORRO
                    SET OBSERVACIONES = @Observaciones,
                        FEC_MODIFICA = dbo.MyGetdate(),
                        USU_MODIFICA = @Usuario
                    WHERE ID_REGISTRO = @IdRegistro;";

        private const string SqlInsertContratoDestino = @"
                    INSERT INTO dbo.fnd_contratos_destinos
                    (
                        cod_plan,
                        cod_operadora,
                        cod_contrato,
                        cod_destino,
                        registro_usuario,
                        registro_fecha
                    )
                    VALUES
                    (
                        @CodPlan,
                        @CodOperadora,
                        @CodContrato,
                        @CodDestino,
                        @Usuario,
                        dbo.MyGetdate()
                    );";

        private const string SqlDeleteContratoDestino = @"
                    DELETE FROM dbo.fnd_contratos_destinos
                    WHERE cod_plan = @CodPlan
                      AND cod_operadora = @CodOperadora
                      AND cod_contrato = @CodContrato
                      AND cod_destino = @CodDestino;";

        #region Destinos

        /// <summary>
        /// Obtiene los destinos disponibles y asignados para un contrato.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="pOperadora">Código de operadora.</param>
        /// <param name="pPlan">Código del plan.</param>
        /// <param name="pContrato">Número de contrato.</param>
        /// <returns>Listado de destinos del contrato.</returns>
        public ErrorDto<List<FndContratoDestinoData>> Fnd_Contratos_Destinos_Obtener(int CodEmpresa, int pOperadora, string pPlan, long pContrato)
        {
            // Paridad VB6 (sbDestinos_Load): exec posicional; contrato 0 si aún no existe
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<FndContratoDestinoData>(
                    $"exec {SpContratoDestinosList} @p1, @p2, @p3",
                    new
                    {
                        p1 = pOperadora,
                        p2 = NormalizarTexto(pPlan),
                        p3 = pContrato
                    }).ToList());
        }

        /// <summary>
        /// Registra los destinos elegidos antes de existir el contrato (paridad VB6: exec posicional de spFnd_Contrato_Destinos_Add).
        /// </summary>
        private void GuardarDestinosPendientes(int codEmpresa, string usuario, ContratosModels contrato)
        {
            if (contrato.destinos_pendientes is null || contrato.destinos_pendientes.Count == 0)
            {
                return;
            }

            var plan = NormalizarTexto(contrato.cod_plan);

            foreach (var destino in contrato.destinos_pendientes)
            {
                var resultado = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
                    connection.QueryFirstOrDefault<long>(
                        $"exec {SpContratoDestinosAdd} @p1, @p2, @p3, @p4, @p5",
                        new
                        {
                            p1 = destino.id_destino,
                            p2 = plan,
                            p3 = contrato.cod_contrato,
                            p4 = NormalizarTexto(destino.observaciones),
                            p5 = NormalizarTexto(usuario)
                        }));

                if (resultado.Code != 0)
                {
                    throw new InvalidOperationException(resultado.Description ?? "Error al registrar el destino del contrato.");
                }
            }
        }

        /// <summary>
        /// Inserta o actualiza la información de un destino de ahorro de contrato.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="destino">Datos del destino de ahorro.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Fnd_Contratos_Destinos_Guardar(int CodEmpresa, FndContratoDestinoData destino)
        {
            if (destino is null)
            {
                return DbHelper.ErrorResponse("Los datos del destino son requeridos.", -2);
            }

            var existe = DbHelper.ExecuteSingleQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlExisteDestinoAhorro,
                0,
                CrearParametrosDestinoAhorro(destino));

            if (existe.Code != 0)
            {
                return DbHelper.ErrorResponse(existe.Description ?? "Error al validar destino.", existe.Code.GetValueOrDefault(-1));
            }

            return DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                existe.Result == 0 ? SqlInsertDestinoAhorro : SqlUpdateDestinoAhorro,
                CrearParametrosDestinoAhorro(destino));
        }

        /// <summary>
        /// Marca o desmarca un destino dentro de la lista de destinos del contrato.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="chkItem">Indica si el destino se asigna o se elimina.</param>
        /// <param name="destino">Datos del destino del contrato.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Fnd_Contratos_DestinosLista_Guardar(int CodEmpresa, bool chkItem, FndContratoDestinoData destino)
        {
            if (destino is null)
            {
                return DbHelper.ErrorResponse("Los datos del destino son requeridos.", -2);
            }

            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                chkItem ? SqlInsertContratoDestino : SqlDeleteContratoDestino,
                CrearParametrosContratoDestino(destino));

            if (result.Code == 0)
            {
                RegistrarBitacoraDestino(CodEmpresa, chkItem, destino);
            }

            return result;
        }

        /// <summary>
        /// Crea los parámetros usados para insertar o actualizar destinos de ahorro.
        /// </summary>
        /// <param name="destino">Datos del destino de ahorro.</param>
        /// <returns>Parámetros seguros para Dapper.</returns>
        private static object CrearParametrosDestinoAhorro(FndContratoDestinoData destino)
        {
            return new
            {
                IdRegistro = destino.id_registro,
                IdDestino = destino.id_destino,
                CodPlan = NormalizarTexto(destino.cod_plan),
                CodContrato = destino.cod_contrato,
                Observaciones = NormalizarTexto(destino.observaciones),
                Usuario = NormalizarTexto(destino.usu_registro)
            };
        }

        /// <summary>
        /// Crea los parámetros usados para asignar o eliminar destinos del contrato.
        /// </summary>
        /// <param name="destino">Datos del destino del contrato.</param>
        /// <returns>Parámetros seguros para Dapper.</returns>
        private static object CrearParametrosContratoDestino(FndContratoDestinoData destino)
        {
            return new
            {
                CodPlan = NormalizarTexto(destino.cod_plan),
                CodOperadora = destino.cod_operadora,
                CodContrato = destino.cod_contrato,
                CodDestino = destino.id_destino,
                Usuario = NormalizarTexto(destino.usu_registro)
            };
        }

        /// <summary>
        /// Registra la bitácora de asignación o eliminación de destinos del contrato.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="asignado">Indica si el destino fue asignado o eliminado.</param>
        /// <param name="destino">Datos del destino modificado.</param>
        private void RegistrarBitacoraDestino(int codEmpresa, bool asignado, FndContratoDestinoData destino)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(destino.usu_modifica),
                DetalleMovimiento = $"Asignación Destino: {destino.id_registro}  P.: {NormalizarTexto(destino.cod_plan)}  Cnt:  {destino.cod_contrato}",
                Movimiento = asignado ? "Aplica - WEB" : "Elimina - WEB",
                Modulo = vModulo
            });
        }

        #endregion
    }
}