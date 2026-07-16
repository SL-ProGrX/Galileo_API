using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralAprDB
    {
        /// <summary>
        /// Guarda la situación financiera del socio: manutención (tipo 'M'), inserción o actualización.
        /// </summary>
        public ErrorDto SituacionFinanciera_Guardar(int CodCliente, AfiBeneSocioFinanzasGuardar finanza)
        {
            try
            {
                if (finanza.tipo == "M")
                {
                    return Manutencion_Guardar(CodCliente, finanza);
                }

                return finanza.id != 0
                    ? SituacionFinanciera_Actualizar(CodCliente, finanza)
                    : SituacionFinanciera_Agregar(CodCliente, finanza);
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        /// <summary>
        /// Inserta un nuevo registro de situación financiera.
        /// </summary>
        private ErrorDto SituacionFinanciera_Agregar(int CodCliente, AfiBeneSocioFinanzasGuardar finanza)
        {
            const string sqlInsert = @"
                INSERT INTO [dbo].[AFI_BENE_SOCIO_FINANZAS]
                    ([CEDULA],[TIPO],[ID_CONCEPTO],[CONCEPTO],[MONTO],[OBSERVACIONES],[ACREEDOR],[DEUDOR],[CUOTA],[SALDO],
                     [MOROSIDAD],[REGISTRA_USUARIO],[REGISTRA_FECHA],[ACTIVO])
                VALUES
                    (@cedula,@tipo,@idConcepto,@concepto,@monto,@observaciones,@acreedor,@deudor,@cuota,@saldo,
                     @morosidad,@registraUsuario,GETDATE(),1)";

            const string sqlId = "SELECT IDENT_CURRENT('AFI_BENE_SOCIO_FINANZAS') AS id";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                connection.Execute(sqlInsert, new
                {
                    cedula = finanza.cedula.Trim(),
                    tipo = finanza.tipo,
                    idConcepto = ItemOrEmpty(finanza.id_concepto),
                    concepto = finanza.concepto,
                    monto = finanza.monto,
                    observaciones = finanza.observaciones,
                    acreedor = finanza.acreedor,
                    deudor = finanza.deudor,
                    cuota = finanza.cuota,
                    saldo = finanza.saldo,
                    morosidad = finanza.morosidad,
                    registraUsuario = finanza.registra_Usuario
                });

                return connection.QueryFirstOrDefault<int>(sqlId);
            });

            return new ErrorDto
            {
                Code = result.Code,
                Description = result.Code == 0 ? result.Result.ToString() : result.Description
            };
        }

        /// <summary>
        /// Actualiza un registro de situación financiera.
        /// </summary>
        private ErrorDto SituacionFinanciera_Actualizar(int CodCliente, AfiBeneSocioFinanzasGuardar finanza)
        {
            const string sqlUpdate = @"
                UPDATE [dbo].[AFI_BENE_SOCIO_FINANZAS]
                   SET [ID_CONCEPTO] = @idConcepto, [CONCEPTO] = @concepto, [MONTO] = @monto, [OBSERVACIONES] = @observaciones,
                       [ACREEDOR] = @acreedor, [DEUDOR] = @deudor, [CUOTA] = @cuota, [SALDO] = @saldo, [MOROSIDAD] = @morosidad,
                       [MODIFICA_USUARIO] = @modificaUsuario, [MODIFICA_FECHA] = GETDATE(), [ACTIVO] = @activo
                 WHERE ID_SITUACIONFINANCIERA = @id";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Execute(sqlUpdate, new
                {
                    idConcepto = ItemOrEmpty(finanza.id_concepto),
                    concepto = finanza.concepto,
                    monto = finanza.monto,
                    observaciones = finanza.observaciones,
                    acreedor = finanza.acreedor,
                    deudor = finanza.deudor,
                    cuota = finanza.cuota,
                    saldo = finanza.saldo,
                    morosidad = finanza.morosidad,
                    modificaUsuario = finanza.modifica_Usuario,
                    activo = finanza.activo ? 1 : 0,
                    id = finanza.id
                }));

            return new ErrorDto
            {
                Code = result.Code,
                Description = result.Code == 0 ? finanza.id.ToString() : result.Description
            };
        }

        /// <summary>
        /// Guarda (inserta o actualiza) el registro de manutención del socio.
        /// </summary>
        private ErrorDto Manutencion_Guardar(int CodCliente, AfiBeneSocioFinanzasGuardar finanza)
        {
            const string sqlExiste = "SELECT COUNT(*) FROM [dbo].[AFI_BENE_SOCIO_FINANZAS] WHERE CEDULA = @cedula AND TIPO = 'M'";
            const string sqlUpdate = @"UPDATE [dbo].[AFI_BENE_SOCIO_FINANZAS]
                                          SET [MONTO] = @monto, [MODIFICA_USUARIO] = @modificaUsuario, [MODIFICA_FECHA] = GETDATE(), [ACTIVO] = 1
                                        WHERE CEDULA = @cedula AND TIPO = 'M'";
            const string sqlInsert = @"
                INSERT INTO [dbo].[AFI_BENE_SOCIO_FINANZAS]
                    ([CEDULA],[TIPO],[ID_CONCEPTO],[CONCEPTO],[MONTO],[OBSERVACIONES],[REGISTRA_USUARIO],[REGISTRA_FECHA],[ACTIVO])
                VALUES (@cedula,@tipo,@idConcepto,@concepto,@monto,@observaciones,@registraUsuario,GETDATE(),1)";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var cedula = finanza.cedula.Trim();
                var count = connection.QueryFirstOrDefault<int>(sqlExiste, new { cedula });

                if (count > 0)
                {
                    connection.Execute(sqlUpdate, new { monto = finanza.monto, modificaUsuario = finanza.modifica_Usuario, cedula });
                }
                else
                {
                    connection.Execute(sqlInsert, new
                    {
                        cedula,
                        tipo = finanza.tipo,
                        idConcepto = ItemOrEmpty(finanza.id_concepto),
                        concepto = finanza.concepto,
                        monto = finanza.monto,
                        observaciones = finanza.observaciones,
                        registraUsuario = finanza.registra_Usuario
                    });
                }

                return true;
            });

            return new ErrorDto
            {
                Code = result.Code,
                Description = result.Code == 0 ? finanza.id.ToString() : result.Description
            };
        }

        /// <summary>
        /// Obtiene los registros de situación financiera del socio por tipo.
        /// </summary>
        public ErrorDto<List<AfiBeneSocioFinanzas>> SituacionFinSocio_Obtener(int CodCliente, string? cedula, string tipo)
        {
            var p = new DynamicParameters();
            p.Add("@tipo", tipo);
            var where = string.Empty;
            if (cedula != null)
            {
                p.Add("@cedula", cedula.Trim());
                where = " TRIM(CEDULA) = @cedula AND ";
            }

            var sql = $@"
                SELECT ID_SITUACIONFINANCIERA AS id, ID_CONCEPTO, CEDULA, TIPO, CONCEPTO, MONTO, OBSERVACIONES, ACTIVO,
                       ACREEDOR, DEUDOR, CUOTA, SALDO, MOROSIDAD
                FROM AFI_BENE_SOCIO_FINANZAS
                WHERE {where} TIPO = @tipo AND ACTIVO = 1";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<AfiBeneSocioFinanzas>(sql, p).ToList());

            return new ErrorDto<List<AfiBeneSocioFinanzas>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<AfiBeneSocioFinanzas>()
            };
        }

        /// <summary>
        /// Inactiva (elimina lógicamente) un registro de situación financiera.
        /// </summary>
        public ErrorDto SituacionFinanciera_Eliminar(int CodCliente, int id, string usuario)
        {
            const string sql = @"UPDATE [dbo].[AFI_BENE_SOCIO_FINANZAS]
                                    SET [ACTIVO] = 0, [MODIFICA_FECHA] = GETDATE(), [MODIFICA_USUARIO] = @usuario
                                  WHERE ID_SITUACIONFINANCIERA = @id";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Execute(sql, new { usuario, id }));

            return new ErrorDto { Code = result.Code, Description = result.Description };
        }

        /// <summary>
        /// Obtiene la síntesis financiera calculada del socio (solo consulta).
        /// </summary>
        public ErrorDto<AfiBeneSintesisFinanzas> SintecisFinanciera_Obtener(int CodCliente, string? cedula)
        {
            if (cedula == null)
            {
                return new ErrorDto<AfiBeneSintesisFinanzas> { Code = 0, Description = "Ok", Result = null };
            }

            const string sql = @"
                SELECT
                    ISNULL((SELECT SUM(MONTO * 0.9016) FROM AFI_BENE_SOCIO_FINANZAS
                            WHERE TIPO = 'I' AND ACTIVO = 1 AND CONCEPTO LIKE '%Salario%' AND CEDULA = @cedula), 0)
                  + ISNULL((SELECT SUM(MONTO) FROM AFI_BENE_SOCIO_FINANZAS
                            WHERE TIPO = 'I' AND ACTIVO = 1 AND CONCEPTO NOT LIKE '%Salario%' AND CEDULA = @cedula), 0) AS INGRESOS,
                    (SELECT SUM(ISNULL(CUOTA,0)) FROM AFI_BENE_SOCIO_FINANZAS WHERE CEDULA = @cedula AND TIPO = 'E' AND ACTIVO = 1) AS ENDEUDAMIENTO,
                    (SELECT SUM(ISNULL(MONTO,0)) FROM AFI_BENE_SOCIO_FINANZAS WHERE CEDULA = @cedula AND TIPO = 'G' AND ACTIVO = 1) AS GASTOS,
                    (SELECT SUM(ISNULL(MONTO,0)) FROM AFI_BENE_SOCIO_FINANZAS WHERE CEDULA = @cedula AND TIPO = 'GE' AND ACTIVO = 1) AS GASTO_ESPECIAL,
                    (SELECT COUNT(CEDULA_PARIENTE) + 1 FROM AFI_BENE_SOCIO_FAMILIA WHERE CEDULA = @cedula AND ACTIVO = 1) AS MIEMBROS,
                    (SELECT SUM(ISNULL(MONTO,0)) FROM AFI_BENE_SOCIO_FINANZAS WHERE CEDULA = @cedula AND TIPO = 'M' AND ACTIVO = 1) AS MANUTENCION";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.QueryFirstOrDefault<AfiBeneSintesisFinanzas>(sql, new { cedula = cedula.Trim() }));

            return new ErrorDto<AfiBeneSintesisFinanzas>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result
            };
        }

        /// <summary>
        /// Obtiene el comportamiento financiero del socio (SP spBene_Situacion_Financiera).
        /// </summary>
        public ErrorDto<AfiBeneCompFinanciero> ComportamientoFinanciero_Obtener(int CodCliente, string cedula)
        {
            const string sql = "EXEC spBene_Situacion_Financiera @cedula";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.QueryFirstOrDefault<AfiBeneCompFinanciero>(sql, new { cedula }));

            return new ErrorDto<AfiBeneCompFinanciero>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result
            };
        }
    }
}
