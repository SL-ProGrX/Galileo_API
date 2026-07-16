using System.Data;
using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class frmAF_Beneficios_Integral_SanDB
    {
        /// <summary>
        /// Guarda la sanción del socio: inserta cuando sancion_id es 0, actualiza en caso contrario.
        /// </summary>
        /// <param name="CodCliente">Código de empresa/cliente.</param>
        /// <param name="sancion">Datos de la sanción.</param>
        /// <returns>Resultado de la operación. En inserto exitoso, Description trae el nuevo sancion_id.</returns>
        public ErrorDto BeneSancionesSocio_Guardar(int CodCliente, AfiBeneSancionesDto sancion)
        {
            try
            {
                if (sancion.sancion_id == 0)
                {
                    return BeneSancion_Insertar(CodCliente, sancion)
                        ? new ErrorDto { Code = 0, Description = sancion.sancion_id.ToString() }
                        : new ErrorDto { Code = -1, Description = "Error al actualizar el dato" };
                }

                return BeneSancion_Actualizar(CodCliente, sancion);
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        /// <summary>
        /// Inserta la sanción del socio mediante el SP de registro y deja traza en bitácora.
        /// </summary>
        private bool BeneSancion_Insertar(int CodCliente, AfiBeneSancionesDto sancion)
        {
            const string procedure = "[spAFI_Bene_Sancion_Registro]";
            const string sqlUltimoId = @"SELECT TOP 1 SANCION_ID FROM AFI_BENE_SANCIONES
                                          WHERE CEDULA = @cedula ORDER BY SANCION_ID DESC";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var values = new
                {
                    Cedula = sancion.cedula,
                    TipoSancion = sancion.tipo_sancion,
                    Activo = sancion.activo ? 1 : 0,
                    Notas = sancion.notas,
                    FechaInicio = sancion.fecha_inicio,
                    FechaCorte = sancion.fecha_corte,
                    Monto = sancion.monto,
                    CodigoCobro = sancion.codigo_cobro,
                    Plazo = sancion.plazo,
                    NOperacion = sancion.n_operacion,
                    RegistroUsuario = sancion.registro_usuario
                };

                connection.Execute(procedure, values, commandType: CommandType.StoredProcedure);

                sancion.sancion_id = connection.QueryFirstOrDefault<int>(
                    sqlUltimoId, new { cedula = sancion.cedula });

                RegistrarBitacora(CodCliente, sancion, "Inserta",
                    $"Inserta datos Sanción COD: [{sancion.sancion_id}]");

                return true;
            });

            return result.Code == 0 && result.Result;
        }

        /// <summary>
        /// Actualiza la sanción del socio (también sirve para activar/desactivar) y deja traza en bitácora.
        /// </summary>
        private ErrorDto BeneSancion_Actualizar(int CodCliente, AfiBeneSancionesDto sancion)
        {
            const string sqlSancion = @"
                UPDATE [dbo].[AFI_BENE_SANCIONES]
                   SET [TIPO_SANCION]     = @tipoSancion,
                       [ACTIVO]           = @activo,
                       [NOTAS]            = @notas,
                       [MONTO]            = @monto,
                       [CODIGO_COBRO]     = @codigoCobro,
                       [PLAZO]            = @plazo,
                       [MODIFICA_FECHA]   = GETDATE(),
                       [MODIFICA_USUARIO] = @registroUsuario
                 WHERE SANCION_ID = @sancionId";

            const string sqlCredito = @"
                UPDATE [dbo].[REG_CREDITOS]
                   SET PLAZO = @plazoCredito
                 WHERE id_solicitud = @nOperacion";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                connection.Execute(sqlSancion, new
                {
                    tipoSancion = sancion.tipo_sancion,
                    activo = sancion.activo ? 1 : 0,
                    notas = sancion.notas,
                    monto = sancion.monto,
                    codigoCobro = sancion.codigo_cobro,
                    plazo = sancion.plazo ?? 0,
                    registroUsuario = sancion.registro_usuario,
                    sancionId = sancion.sancion_id
                });

                connection.Execute(sqlCredito, new
                {
                    plazoCredito = sancion.plazocredito,
                    nOperacion = sancion.n_operacion
                });

                RegistrarBitacora(CodCliente, sancion, "Actualiza",
                    $"Actualiza datos Sanción COD: [{sancion.sancion_id}]");

                return true;
            });

            return new ErrorDto { Code = result.Code, Description = result.Description };
        }

        /// <summary>
        /// Registra el movimiento de la sanción en la bitácora de beneficios (helper compartido).
        /// </summary>
        private void RegistrarBitacora(int CodCliente, AfiBeneSancionesDto sancion, string movimiento, string detalle)
        {
            _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDto
            {
                EmpresaId = CodCliente,
                cod_beneficio = sancion.cod_beneficio ?? string.Empty,
                consec = sancion.consec,
                movimiento = movimiento,
                detalle = detalle,
                registro_usuario = sancion.registro_usuario
            });
        }
    }
}
