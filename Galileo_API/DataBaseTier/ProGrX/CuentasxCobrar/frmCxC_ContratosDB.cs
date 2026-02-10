using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCContratosDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _dbBitacora;
        private const int vModulo = 31;
        private readonly string vError = "Error";

        public FrmCxCContratosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _dbBitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Registra un movimiento en la bitácora del sistema.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="usuario">Usuario que realiza la acción.</param>
        /// <param name="detalle">Detalle del movimiento.</param>
        /// <param name="movimiento">Tipo de movimiento (Registra, Modifica, Elimina).</param>
        private void RegistrarBitacora(int codEmpresa, string usuario, string detalle, string movimiento)
        {
            _dbBitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        /// <summary>
        /// Consulta la lista de contratos para búsquedas.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de contratos con código y descripción.</returns>
        public ErrorDto<List<ContratoBusquedaDto>> Contratos_Busqueda_Lista(int codEmpresa)
        {
            var query = @"SELECT cod_contrato AS Contrato, Descripcion FROM CxC_Contratos ORDER BY cod_contrato";
            return DbHelper.ExecuteListQuery<ContratoBusquedaDto>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene el detalle de un contrato por su código.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContrato">Código del contrato.</param>
        /// <returns>Detalle del contrato.</returns>
        public ErrorDto<ContratoDetalleDto?> Contrato_ObtenerPorCodigo(int codEmpresa, string codContrato)
        {
            var query = @"SELECT 
                            COD_CONTRATO as Cod_Contrato,
                            DESCRIPCION,
                            NOTAS,
                            ACTIVO,
                            PLAZO,
                            TASA_CORRIENTE as Tasa_Corriente,
                            TASA_MORA as Tasa_Mora,
                            REGISTRO_FECHA,
                            REGISTRO_USUARIO,
                            ACTUALIZA_FECHA,
                            ACTUALIZA_USUARIO,
                            SUSCRIPCION_ABIERTA as Suscripcion_Abierta,
                            PAGADORES_ABIERTO as Pagadores_Abierto
                          FROM CxC_Contratos
                          WHERE COD_CONTRATO = @CodContrato";
            return DbHelper.ExecuteSingleQuery<ContratoDetalleDto>(_portalDb, codEmpresa, query, default, new { CodContrato = codContrato });
        }

        /// <summary>
        /// Obtiene las personas asociadas a un contrato.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContrato">Código del contrato.</param>
        /// <returns>Lista de personas asociadas al contrato.</returns>
        public ErrorDto<List<ContratoPersonaDto>> Contrato_PersonasPorContrato(int codEmpresa, string codContrato)
        {
            var query = @"
                SELECT 
                    P.nombre AS Nombre,
                    C.COD_CONTRATO AS Cod_Contrato,
                    C.CEDULA,
                    C.ACTIVO,
                    C.PLAZO,
                    C.TASA_CORRIENTE AS Tasa_Corriente,
                    C.TASA_MORA AS Tasa_Mora,
                    C.NOTAS,
                    C.REGISTRO_FECHA,
                    C.REGISTRO_USUARIO,
                    C.ACTUALIZA_FECHA,
                    C.ACTUALIZA_USUARIO,
                    C.CONTRATO_NUM,
                    C.CONTRATO_VENCE,
                    C.CONTRATO_TIPO
                FROM CxC_Personas P
                INNER JOIN CxC_Personas_Contratos C ON P.cedula = C.cedula
                WHERE C.cod_contrato = @CodContrato";
            return DbHelper.ExecuteListQuery<ContratoPersonaDto>(_portalDb, codEmpresa, query, new { CodContrato = codContrato });
        }

        /// <summary>
        /// Elimina un pagador asociado a un contrato en la tabla CxC_Personas_Contratos_Pagadores.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros con código de contrato y cédula.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> Contrato_PersonaPagador_Eliminar(int codEmpresa, ContratoPersonaDeleteParams param)
        {
            var query = @"DELETE FROM CxC_Personas_Contratos_Pagadores WHERE cod_contrato = @Cod_Contrato AND cedula = @Cedula";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);
            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? vError, result.Code ?? -1, false);
        }

        /// <summary>
        /// Elimina una suscripción asociada a un contrato en la tabla CxC_Personas_Contratos_Suscripciones.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros con código de contrato y cédula.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> Contrato_PersonaSuscripcion_Eliminar(int codEmpresa, ContratoPersonaDeleteParams param)
        {
            var query = @"DELETE FROM CxC_Personas_Contratos_Suscripciones WHERE cod_contrato = @Cod_Contrato AND cedula = @Cedula";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);
            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? vError, result.Code ?? -1, false);
        }

        /// <summary>
        /// Elimina una persona asociada a un contrato en la tabla CxC_Personas_Contratos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros con código de contrato y cédula.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> Contrato_Persona_Eliminar(int codEmpresa, ContratoPersonaDeleteParams param)
        {
            var query = @"DELETE FROM CxC_Personas_Contratos WHERE cod_contrato = @Cod_Contrato AND cedula = @Cedula";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);
            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? vError, result.Code ?? -1, false);
        }

        /// <summary>
        /// Obtiene los pagadores asociados a un contrato.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContrato">Código del contrato.</param>
        /// <returns>Lista de pagadores asociados al contrato.</returns>
        public ErrorDto<List<ContratoPagadorDto>> Contrato_PagadoresPorContrato(int codEmpresa, string codContrato)
        {
            var query = @"
                SELECT 
                    P.nombre AS Nombre,
                    C.COD_CONTRATO AS Cod_Contrato,
                    C.CEDULA,
                    C.REGISTRO_FECHA,
                    C.REGISTRO_USUARIO
                FROM CxC_Personas P
                INNER JOIN CxC_Contratos_Pagadores C ON P.cedula = C.cedula
                WHERE C.cod_contrato = @CodContrato";
            return DbHelper.ExecuteListQuery<ContratoPagadorDto>(_portalDb, codEmpresa, query, new { CodContrato = codContrato });
        }

        /// <summary>
        /// Elimina un pagador de la tabla CxC_Contratos_Pagadores y registra en bitácora.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros con código de contrato, cédula y usuario.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> Contrato_Pagador_Eliminar(int codEmpresa, ContratoPersonaDeleteParams param)
        {
            var query = @"DELETE FROM CxC_Contratos_Pagadores WHERE cod_contrato = @Cod_Contrato AND cedula = @Cedula";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);

            if (result.Code == 0)
                RegistrarBitacora(
                    codEmpresa,
                    param.Usuario!,
                    $"Pagador Id.: {param.Cedula} de Contrato No.: {param.Cod_Contrato}",
                    "Borra - WEB"
                );

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? vError, result.Code ?? -1, false);
        }

        /// <summary>
        /// Obtiene los cargos asociados a un contrato.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContrato">Código del contrato.</param>
        /// <returns>Lista de cargos asociados al contrato.</returns>
        public ErrorDto<List<ContratoCargoDto>> Contrato_CargosPorContrato(int codEmpresa, string codContrato)
        {
            var query = @"
                SELECT 
                    C.descripcion AS Descripcion,
                    S.COD_CONTRATO AS Cod_Contrato,
                    S.COD_CARGO AS Cod_Cargo,
                    S.TIPO,
                    S.VALOR,
                    S.FRECUENCIA_TIPO,
                    S.FRECUENCIA_DIAS,
                    S.MODIFICA,
                    S.REGISTRO_FECHA,
                    S.REGISTRO_USUARIO
                FROM CxC_Cargos C
                INNER JOIN CxC_Contratos_Cargos S ON C.cod_cargo = S.cod_cargo
                WHERE S.cod_contrato = @CodContrato";
            return DbHelper.ExecuteListQuery<ContratoCargoDto>(_portalDb, codEmpresa, query, new { CodContrato = codContrato });
        }

        /// <summary>
        /// Elimina un cargo asociado a un contrato y registra en bitácora.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros con código de contrato, código de cargo y usuario.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> Contrato_Cargo_Eliminar(int codEmpresa, ContratoCargoDeleteParams param)
        {
            var query = @"DELETE FROM CxC_Contratos_Cargos WHERE cod_contrato = @Cod_Contrato AND cod_cargo = @Cod_Cargo";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);

            if (result.Code == 0)
                RegistrarBitacora(
                    codEmpresa,
                    param.Usuario ?? "",
                    $"Cargo Suscripción Cod:{param.Cod_Cargo} Cnt: {param.Cod_Contrato}",
                    "Borra - WEB"
                );

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? vError, result.Code ?? -1, false);
        }

        /// <summary>
        /// Obtiene los conceptos asociados a un contrato.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContrato">Código del contrato.</param>
        /// <returns>Lista de conceptos asociados al contrato.</returns>
        public ErrorDto<List<ContratoConceptoDto>> Contrato_ConceptosPorContrato(int codEmpresa, string codContrato)
        {
            var query = @"
                SELECT 
                    C.cod_concepto AS Codigo,
                    C.descripcion,
                    S.cod_contrato AS Cod_Contrato,
                    S.cod_concepto AS Cod_Concepto,
                    S.registro_fecha AS Registro_Fecha,
                    S.registro_usuario AS Registro_Usuario
                FROM CxC_Conceptos C
                LEFT JOIN CxC_Conceptos_Contratos S
                  ON C.cod_concepto = S.cod_concepto
                 AND S.cod_contrato = @CodContrato";
            return DbHelper.ExecuteListQuery<ContratoConceptoDto>(_portalDb, codEmpresa, query, new { CodContrato = codContrato });
        }

        /// <summary>
        /// Inserta un concepto asociado a un contrato y registra en bitácora.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros con código de contrato, código de concepto y usuario.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> Contrato_Concepto_Insertar(int codEmpresa, ContratoConceptoParams param)
        {
            var query = @"
                INSERT INTO CxC_Conceptos_Contratos (cod_concepto, cod_contrato, registro_usuario, registro_fecha)
                VALUES (@Cod_Concepto, @Cod_Contrato, @Usuario, dbo.MyGetdate())";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);

            if (result.Code == 0)
                RegistrarBitacora(
                    codEmpresa,
                    param.Usuario ?? "",
                    $"Asocia Concepto Cod:{param.Cod_Concepto} a Contrato No.: {param.Cod_Contrato}",
                    "Registra - WEB"
                );

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? vError, result.Code ?? -1, false);
        }

        /// <summary>
        /// Elimina un concepto asociado a un contrato y registra en bitácora.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros con código de contrato, código de concepto y usuario.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> Contrato_Concepto_Eliminar(int codEmpresa, ContratoConceptoParams param)
        {
            var query = @"DELETE FROM CxC_Conceptos_Contratos WHERE cod_concepto = @Cod_Concepto AND cod_contrato = @Cod_Contrato";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);

            if (result.Code == 0)
                RegistrarBitacora(
                    codEmpresa,
                    param.Usuario ?? "",
                    $"Borra Concepto Cod:{param.Cod_Concepto} de Contrato No.: {param.Cod_Contrato}",
                    "Borra - WEB"
                );

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? vError, result.Code ?? -1, false);
        }

        /// <summary>
        /// Inserta un nuevo contrato y registra en bitácora.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del contrato.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> Contrato_Insertar(int codEmpresa, ContratoSaveParams param)
        {
            var query = @"
        INSERT INTO CxC_Contratos
        (cod_contrato, descripcion, Notas, Activo, Plazo, Tasa_Corriente, Tasa_Mora,
         SUSCRIPCION_ABIERTA, PAGADORES_ABIERTO, registro_usuario, registro_fecha)
        VALUES
        (@Cod_Contrato, @Descripcion, @Notas, @Activo, @Plazo, @Tasa_Corriente, @Tasa_Mora,
         @Suscripcion_Abierta, @Pagadores_Abierto, @Usuario, dbo.MyGetdate())";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);

            if (result.Code == 0)
                RegistrarBitacora(codEmpresa, param.Usuario, $"Contrato No.: {param.Cod_Contrato}", "Registra - WEB");

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? vError, result.Code ?? -1, false);
        }

        /// <summary>
        /// Actualiza un contrato existente y registra en bitácora.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del contrato.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> Contrato_Actualizar(int codEmpresa, ContratoSaveParams param)
        {
            var query = @"
        UPDATE CxC_Contratos
        SET descripcion = @Descripcion,
            Notas = @Notas,
            Activo = @Activo,
            Plazo = @Plazo,
            Tasa_Corriente = @Tasa_Corriente,
            Tasa_Mora = @Tasa_Mora,
            Actualiza_Usuario = @Usuario,
            Actualiza_Fecha = dbo.MyGetdate(),
            SUSCRIPCION_ABIERTA = @Suscripcion_Abierta,
            PAGADORES_ABIERTO = @Pagadores_Abierto
        WHERE cod_contrato = @Cod_Contrato";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);

            if (result.Code == 0)
                RegistrarBitacora(codEmpresa, param.Usuario, $"Contrato No.: {param.Cod_Contrato}", "Modifica - WEB");

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? vError, result.Code ?? -1, false);
        }

        /// <summary>
        /// Elimina un contrato y registra en bitácora.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros con código de contrato y usuario.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> Contrato_Eliminar(int codEmpresa, ContratoDeleteParams param)
        {
            var query = @"DELETE FROM CxC_Contratos WHERE cod_contrato = @Cod_Contrato";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);

            if (result.Code == 0)
                RegistrarBitacora(codEmpresa, param.Usuario, $"Contrato No.: {param.Cod_Contrato}", "Elimina - WEB");

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? vError, result.Code ?? -1, false);
        }
    }
}
