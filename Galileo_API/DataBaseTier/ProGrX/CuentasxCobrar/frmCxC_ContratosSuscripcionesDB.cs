using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCContratosSuscripcionesDB
    {
        private readonly PortalDB _portalDb;

        public FrmCxCContratosSuscripcionesDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Consulta todas las personas registradas en CxC_Personas.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de personas (cedula, nombre).</returns>
        public ErrorDto<List<CxcPersonaDto>> CxcPersonas_Lista(int codEmpresa)
        {
            var query = @"SELECT cedula AS Cedula, nombre AS Nombre FROM CxC_Personas";
            return DbHelper.ExecuteListQuery<CxcPersonaDto>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Consulta la información de una persona y su contrato.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <param name="codContrato">Código del contrato.</param>
        /// <returns>Datos de la persona y contrato.</returns>
        public ErrorDto<CxcPersonaContratoDto?> CxcPersonaContrato_Obtener(int codEmpresa, string cedula, string codContrato)
        {
            var query = @"
                SELECT 
                    P.nombre AS Nombre,
                    C.COD_CONTRATO AS Cod_Contrato,
                    C.CEDULA AS Cedula,
                    C.ACTIVO AS Activo,
                    C.PLAZO AS Plazo,
                    C.TASA_CORRIENTE AS Tasa_Corriente,
                    C.TASA_MORA AS Tasa_Mora,
                    C.NOTAS AS Notas,
                    C.REGISTRO_FECHA AS Registro_Fecha,
                    C.REGISTRO_USUARIO AS Registro_Usuario,
                    C.ACTUALIZA_FECHA AS Actualiza_Fecha,
                    C.ACTUALIZA_USUARIO AS Actualiza_Usuario,
                    C.CONTRATO_NUM AS Contrato_Num,
                    C.CONTRATO_VENCE AS Contrato_Vence,
                    C.CONTRATO_TIPO AS Contrato_Tipo
                FROM CxC_Personas P
                INNER JOIN CxC_Personas_Contratos C ON P.cedula = C.cedula
                WHERE C.cedula = @Cedula AND C.cod_contrato = @CodContrato";
            return DbHelper.ExecuteSingleQuery<CxcPersonaContratoDto>(_portalDb, codEmpresa, query, default, new { Cedula = cedula, CodContrato = codContrato });
        }

        /// <summary>
        /// Guarda (inserta o actualiza) un registro de persona-contrato.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del contrato/persona.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CxcPersonaContrato_Guardar(int codEmpresa, CxcPersonaContratoSaveParams param)
        {
            // Verifica existencia
            var existe = DbHelper.ExecuteSingleQuery<int>(
                _portalDb, codEmpresa,
                "SELECT COUNT(1) FROM CxC_Personas_Contratos WHERE cedula = @Cedula AND cod_contrato = @Cod_Contrato",
                default, new { param.Contrato.Cedula, param.Contrato.Cod_Contrato }
            ).Result;

            if (existe == 0)
            {
                // Insertar
                var sql = @"
                    INSERT INTO CxC_Personas_Contratos
                    (cedula, cod_contrato, Notas, Activo, Plazo, Tasa_Corriente, Tasa_Mora,
                     registro_usuario, registro_fecha, Contrato_Num, Contrato_Tipo, Contrato_Vence)
                    VALUES
                    (@Cedula, @Cod_Contrato, @Notas, @Activo, @Plazo, @Tasa_Corriente, @Tasa_Mora,
                     @Usuario, dbo.MyGetdate(), @Contrato_Num, @Contrato_Tipo, @Contrato_Vence)";
                return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    var rows = conn.Execute(sql, param);
                    return rows > 0;
                });
            }
            else
            {
                // Actualizar
                var sql = @"
                    UPDATE CxC_Personas_Contratos
                    SET
                        Notas = @Notas,
                        Activo = @Activo,
                        Plazo = @Plazo,
                        Tasa_Corriente = @Tasa_Corriente,
                        Tasa_Mora = @Tasa_Mora,
                        Actualiza_Usuario = @Usuario,
                        Actualiza_Fecha = dbo.MyGetdate(),
                        Contrato_Num = @Contrato_Num,
                        Contrato_Tipo = @Contrato_Tipo,
                        Contrato_Vence = @Contrato_Vence
                    WHERE cedula = @Cedula AND cod_contrato = @Cod_Contrato";
                return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    var rows = conn.Execute(sql, param);
                    return rows > 0;
                });
            }
        }

        /// <summary>
        /// Elimina un registro de persona-contrato.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de eliminación.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CxcPersonaContrato_Eliminar(int codEmpresa, CxcPersonaContratoDeleteParams param)
        {
            var sql = @"DELETE FROM CxC_Personas_Contratos WHERE cedula = @Cedula AND cod_contrato = @Cod_Contrato";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });
        }

        /// <summary>
        /// Consulta los pagadores asignados a un contrato/persona.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContrato">Código del contrato.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <returns>Lista de pagadores asignados.</returns>
        public ErrorDto<List<CxcPersonaContratoPagadorDto>> CxcPersonaContratoPagadores_Lista(int codEmpresa, string codContrato, string cedula)
        {
            var query = @"
                SELECT 
                    P.nombre AS Nombre,
                    C.COD_CONTRATO AS Cod_Contrato,
                    C.CEDULA AS Cedula,
                    C.CEDULA_PAGADOR AS Cedula_Pagador,
                    C.REGISTRO_FECHA AS Registro_Fecha,
                    C.REGISTRO_USUARIO AS Registro_Usuario
                FROM CxC_Personas P
                INNER JOIN CxC_Personas_Contratos_Pagadores C
                  ON P.cedula = C.cedula_pagador
                WHERE C.cod_contrato = @Cod_Contrato
                  AND C.cedula = @Cedula";
            return DbHelper.ExecuteListQuery<CxcPersonaContratoPagadorDto>(_portalDb, codEmpresa, query, new { Cod_Contrato = codContrato, Cedula = cedula });
        }

        /// <summary>
        /// Consulta los pagadores disponibles para asignar a un contrato/persona.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContrato">Código del contrato.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <returns>Lista de pagadores disponibles.</returns>
        public ErrorDto<List<CxcPersonaContratoPagadorDto>> CxcContratoPagadoresDisponibles_Lista(int codEmpresa, string codContrato, string cedula)
        {
            var query = @"
                SELECT 
                    P.nombre AS Nombre,
                    C.COD_CONTRATO AS Cod_Contrato,
                    C.CEDULA AS Cedula,
                    C.REGISTRO_FECHA AS Registro_Fecha,
                    C.REGISTRO_USUARIO AS Registro_Usuario
                FROM CxC_Personas P
                INNER JOIN CxC_Contratos_Pagadores C
                  ON P.cedula = C.cedula
                WHERE C.cod_contrato = @Cod_Contrato
                  AND C.cedula <> @Cedula
                  AND C.cedula NOT IN (
                      SELECT Cedula_Pagador
                      FROM CxC_Personas_Contratos_Pagadores
                      WHERE Cedula = @Cedula
                        AND Cod_Contrato = @Cod_Contrato
                  )";
            return DbHelper.ExecuteListQuery<CxcPersonaContratoPagadorDto>(_portalDb, codEmpresa, query, new { Cod_Contrato = codContrato, Cedula = cedula });
        }

        /// <summary>
        /// Inserta un pagador en CxC_Personas_Contratos_Pagadores.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del pagador.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CxcPersonaContratoPagador_Insertar(int codEmpresa, CxcPersonaContratoPagadorSaveParams param)
        {
            var sql = @"
        INSERT INTO CxC_Personas_Contratos_Pagadores
        (cod_contrato, cedula, cedula_pagador, registro_fecha, registro_usuario)
        VALUES
        (@Cod_Contrato, @Cedula, @Cedula_Pagador, dbo.MyGetdate(), @Registro_Usuario)";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });
        }

        /// <summary>
        /// Elimina un pagador de CxC_Personas_Contratos_Pagadores.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de eliminación.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CxcPersonaContratoPagador_Eliminar(int codEmpresa, CxcPersonaContratoPagadorDeleteParams param)
        {
            var sql = @"
        DELETE FROM CxC_Personas_Contratos_Pagadores
        WHERE cod_contrato = @Cod_Contrato
          AND cedula = @Cedula
          AND cedula_pagador = @Cedula_Pagador";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });
        }

        /// <summary>
        /// Consulta las suscripciones de una persona a un contrato.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContrato">Código del contrato.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <returns>Lista de suscripciones.</returns>
        public ErrorDto<List<CxcPersonaContratoSuscripcionDto>> CxcPersonaContratoSuscripciones_Lista(int codEmpresa, string codContrato, string cedula)
        {
            var query = @"
        SELECT 
            C.descripcion AS Descripcion,
            S.COD_CONTRATO AS Cod_Contrato,
            S.CEDULA AS Cedula,
            S.COD_CARGO AS Cod_Cargo,
            S.FRECUENCIA_DIAS AS Frecuencia_Dias,
            S.PAGO_ULTIMO AS Pago_Ultimo,
            S.PAGO_PROXIMO AS Pago_Proximo,
            S.RECAUDADO AS Recaudado,
            S.FRECUENCIA_TIPO AS Frecuencia_Tipo,
            S.TIPO AS Tipo,
            S.VALOR AS Valor,
            S.MODIFICA AS Modifica,
            S.REGISTRO_FECHA AS Registro_Fecha,
            S.REGISTRO_USUARIO AS Registro_Usuario
        FROM CxC_Cargos C
        INNER JOIN CxC_Personas_Contratos_Suscripciones S
          ON C.cod_cargo = S.cod_cargo
        WHERE S.cod_contrato = @Cod_Contrato
          AND S.cedula = @Cedula";
            return DbHelper.ExecuteListQuery<CxcPersonaContratoSuscripcionDto>(_portalDb, codEmpresa, query, new { Cod_Contrato = codContrato, Cedula = cedula });
        }

        /// <summary>
        /// Consulta los cargos disponibles para suscribir a una persona en un contrato.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContrato">Código del contrato.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <returns>Lista de cargos disponibles para suscripción.</returns>
        public ErrorDto<List<CxcPersonaContratoSuscripcionDto>> CxcContratoCargosDisponibles_Lista(int codEmpresa, string codContrato, string cedula)
        {
            var query = @"
        SELECT 
            C.descripcion AS Descripcion,
            S.COD_CONTRATO AS Cod_Contrato,
            @Cedula AS Cedula,
            S.COD_CARGO AS Cod_Cargo,
            S.FRECUENCIA_DIAS AS Frecuencia_Dias,
            dbo.MyGetdate() AS Pago_Ultimo,
            DATEADD(d, S.FRECUENCIA_DIAS, dbo.MyGetdate()) AS Pago_Proximo,
            0 AS Recaudado,
            S.FRECUENCIA_TIPO AS Frecuencia_Tipo,
            S.TIPO AS Tipo,
            S.VALOR AS Valor,
            S.MODIFICA AS Modifica,
            S.REGISTRO_FECHA AS Registro_Fecha,
            S.REGISTRO_USUARIO AS Registro_Usuario
        FROM CxC_Cargos C
        INNER JOIN CxC_Contratos_Cargos S
          ON C.cod_cargo = S.cod_cargo
        WHERE S.cod_contrato = @Cod_Contrato
          AND S.cod_cargo NOT IN (
              SELECT cod_cargo
              FROM CxC_Personas_Contratos_Suscripciones
              WHERE cod_contrato = @Cod_Contrato
                AND cedula = @Cedula
          )";
            return DbHelper.ExecuteListQuery<CxcPersonaContratoSuscripcionDto>(_portalDb, codEmpresa, query, new { Cod_Contrato = codContrato, Cedula = cedula });
        }

        /// <summary>
        /// Inserta una suscripción en CxC_Personas_Contratos_Suscripciones.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de la suscripción.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CxcPersonaContratoSuscripcion_Insertar(int codEmpresa, CxcPersonaContratoSuscripcionSaveParams param)
        {
            var sql = @"
        INSERT INTO CxC_Personas_Contratos_Suscripciones
        (cod_contrato, cedula, cod_cargo, Tipo, valor, frecuencia_Tipo,
         frecuencia_dias, recaudado, pago_ultimo, pago_proximo, modifica,
         registro_Fecha, Registro_Usuario)
        VALUES
        (@Cod_Contrato, @Cedula, @Cod_Cargo, @Tipo, @Valor, @Frecuencia_Tipo,
         @Frecuencia_Dias, @Recaudado, @Pago_Ultimo, @Pago_Proximo, @Modifica,
         dbo.MyGetdate(), @Registro_Usuario)";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });
        }

        /// <summary>
        /// Elimina una suscripción de CxC_Personas_Contratos_Suscripciones.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de eliminación.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CxcPersonaContratoSuscripcion_Eliminar(int codEmpresa, CxcPersonaContratoSuscripcionDeleteParams param)
        {
            var sql = @"
        DELETE FROM CxC_Personas_Contratos_Suscripciones
        WHERE cod_contrato = @Cod_Contrato
          AND cedula = @Cedula
          AND cod_cargo = @Cod_Cargo";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });
        }
    }
}
