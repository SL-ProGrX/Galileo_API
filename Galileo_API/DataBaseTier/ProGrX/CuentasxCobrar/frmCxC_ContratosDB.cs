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
        public ErrorDto<List<ContratoBusquedaDto>> Contratos_Busqueda_Lista(int codEmpresa)
        {
            var query = @"SELECT cod_contrato AS Contrato, Descripcion FROM CxC_Contratos ORDER BY cod_contrato";
            return DbHelper.ExecuteListQuery<ContratoBusquedaDto>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene el detalle de un contrato por su código.
        /// </summary>
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

        public ErrorDto<bool> Contrato_PersonaPagador_Eliminar(int codEmpresa, ContratoPersonaDeleteParams param)
        {
            var query = @"DELETE FROM CxC_Personas_Contratos_Pagadores WHERE cod_contrato = @Cod_Contrato AND cedula = @Cedula";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);
            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? vError, result.Code ?? -1, false);
        }

        public ErrorDto<bool> Contrato_PersonaSuscripcion_Eliminar(int codEmpresa, ContratoPersonaDeleteParams param)
        {
            var query = @"DELETE FROM CxC_Personas_Contratos_Suscripciones WHERE cod_contrato = @Cod_Contrato AND cedula = @Cedula";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);
            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? vError, result.Code ?? -1, false);
        }

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

        public ErrorDto<bool> Contrato_Pagador_Eliminar(int codEmpresa, ContratoPersonaDeleteParams param)
        {
            var query = @"DELETE FROM CxC_Contratos_Pagadores WHERE cod_contrato = @Cod_Contrato AND cedula = @Cedula";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);

            if (result.Code == 0)
                RegistrarBitacora(
                    codEmpresa,
                    param.Usuario,
                    $"Pagador Id.: {param.Cedula} de Contrato No.: {param.Cod_Contrato}",
                    "Borra - WEB"
                );

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? vError, result.Code ?? -1, false);
        }

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
    }
}
