using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralCreDB
    {
        // ==================== Crece ====================

        /// <summary>
        /// Guarda el registro Crece: inserta (id_crece 0) o actualiza.
        /// </summary>
        public ErrorDto BeneSocioCrece_Guardar(int CodCliente, AfiBeneSocioCreceDto beneficio)
        {
            try
            {
                return beneficio.id_crece > 0
                    ? BeneSocioCrece_Actualizar(CodCliente, beneficio)
                    : BeneSocioCrece_Insertar(CodCliente, beneficio);
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        /// <summary>
        /// Inserta un nuevo registro Crece y devuelve el id generado en Description.
        /// </summary>
        private ErrorDto BeneSocioCrece_Insertar(int CodCliente, AfiBeneSocioCreceDto beneficio)
        {
            const string sqlInsert = @"
                INSERT INTO [dbo].[AFI_BENE_SOCIO_CRECE]
                    ([COD_BENEFICIO],[CONSEC],[CAPACITACION_CMP],[APLICA_PRODUCTO],[COUTA_INICIAL],[COUTA_APLICAR],
                     [AHORRO],[LIQUIDEZ],[OBSERVACIONES_PROD],[APLICA_BENE],[MONTO_PRIMERA_TARJETA],[ENTREGA_PRIMERA_TARJETA],
                     [MONTO_SEGUNDA_TARJETA],[ENTREGA_SEGUNDA_TARJETA],[REGISTRO_FECHA],[REGISTRO_USUARIO],[OBSERVACIONES_BENE],
                     [fecha_cuota_inicial],[fecha_cuota_aplicar],[fecha_ahorro])
                VALUES
                    (@codBeneficio,@consec,@capacitacionCmp,@aplicaProducto,@coutaInicial,@coutaAplicar,
                     @ahorro,@liquidez,@observacionesProd,@aplicaBene,@montoPrimeraTarjeta,@entregaPrimeraTarjeta,
                     @montoSegundaTarjeta,@entregaSegundaTarjeta,GETDATE(),@registroUsuario,@observacionesBene,
                     @fechaCuotaInicial,@fechaCuotaAplicar,@fechaAhorro)";

            const string sqlId = "SELECT IDENT_CURRENT('AFI_BENE_SOCIO_CRECE') AS id";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                connection.Execute(sqlInsert, new
                {
                    codBeneficio = beneficio.cod_beneficio,
                    consec = beneficio.consec,
                    capacitacionCmp = beneficio.capacitacion_cmp ? 1 : 0,
                    aplicaProducto = beneficio.aplica_producto ? 1 : 0,
                    coutaInicial = beneficio.couta_inicial,
                    coutaAplicar = beneficio.couta_aplicar,
                    ahorro = beneficio.ahorro,
                    liquidez = beneficio.liquidez,
                    observacionesProd = beneficio.observaciones_prod,
                    aplicaBene = beneficio.aplica_bene ? 1 : 0,
                    montoPrimeraTarjeta = beneficio.monto_primera_tarjeta,
                    entregaPrimeraTarjeta = beneficio.entrega_primera_tarjeta ? 1 : 0,
                    montoSegundaTarjeta = beneficio.monto_segunda_tarjeta,
                    entregaSegundaTarjeta = beneficio.entrega_segunda_tarjeta ? 1 : 0,
                    registroUsuario = beneficio.registro_usuario,
                    observacionesBene = beneficio.observaciones_bene,
                    fechaCuotaInicial = beneficio.fecha_cuota_inicial,
                    fechaCuotaAplicar = beneficio.fecha_cuota_aplicar,
                    fechaAhorro = beneficio.fecha_ahorro
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
        /// Actualiza el registro Crece y procesa las tarjetas/montos asociados.
        /// </summary>
        private ErrorDto BeneSocioCrece_Actualizar(int CodCliente, AfiBeneSocioCreceDto beneficio)
        {
            if (beneficio.monto_segunda_tarjeta != 0 && !beneficio.entrega_primera_tarjeta)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "No se puede ingresar monto de segunda tarjeta sin haber entregado la primera"
                };
            }

            const string sqlUpdate = @"
                UPDATE [dbo].[AFI_BENE_SOCIO_CRECE]
                   SET [CAPACITACION_CMP]       = @capacitacionCmp,
                       [APLICA_PRODUCTO]        = @aplicaProducto,
                       [COUTA_INICIAL]          = @coutaInicial,
                       [COUTA_APLICAR]          = @coutaAplicar,
                       [AHORRO]                 = @ahorro,
                       [LIQUIDEZ]               = @liquidez,
                       [OBSERVACIONES_PROD]     = @observacionesProd,
                       [OBSERVACIONES_BENE]     = @observacionesBene,
                       [APLICA_BENE]            = @aplicaBene,
                       [MONTO_PRIMERA_TARJETA]  = @montoPrimeraTarjeta,
                       [ENTREGA_PRIMERA_TARJETA]= @entregaPrimeraTarjeta,
                       [MONTO_SEGUNDA_TARJETA]  = @montoSegundaTarjeta,
                       [ENTREGA_SEGUNDA_TARJETA]= @entregaSegundaTarjeta,
                       [MODIFICA_FECHA]         = GETDATE(),
                       [fecha_cuota_inicial]    = @fechaCuotaInicial,
                       [fecha_cuota_aplicar]    = @fechaCuotaAplicar,
                       [fecha_ahorro]           = @fechaAhorro,
                       [MODIFICA_USUARIO]       = @modificaUsuario
                 WHERE ID_CRECE = @idCrece";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                connection.Execute(sqlUpdate, new
                {
                    capacitacionCmp = beneficio.capacitacion_cmp ? 1 : 0,
                    aplicaProducto = beneficio.aplica_producto ? 1 : 0,
                    coutaInicial = beneficio.couta_inicial,
                    coutaAplicar = beneficio.couta_aplicar,
                    ahorro = beneficio.ahorro,
                    liquidez = beneficio.liquidez,
                    observacionesProd = beneficio.observaciones_prod,
                    observacionesBene = beneficio.observaciones_bene,
                    aplicaBene = beneficio.aplica_bene ? 1 : 0,
                    montoPrimeraTarjeta = beneficio.monto_primera_tarjeta,
                    entregaPrimeraTarjeta = beneficio.entrega_primera_tarjeta ? 1 : 0,
                    montoSegundaTarjeta = beneficio.monto_segunda_tarjeta,
                    entregaSegundaTarjeta = beneficio.entrega_segunda_tarjeta ? 1 : 0,
                    fechaCuotaInicial = beneficio.fecha_cuota_inicial,
                    fechaCuotaAplicar = beneficio.fecha_cuota_aplicar,
                    fechaAhorro = beneficio.fecha_ahorro,
                    modificaUsuario = beneficio.modifica_usuario,
                    idCrece = beneficio.id_crece
                });

                ProcesarTarjetas(connection, CodCliente, beneficio);
                return beneficio.id_crece;
            });

            return new ErrorDto
            {
                Code = result.Code,
                Description = result.Code == 0 ? beneficio.id_crece.ToString() : result.Description
            };
        }

        /// <summary>
        /// Decide el procesamiento de tarjetas según exista o no monto en la primera tarjeta.
        /// </summary>
        private void ProcesarTarjetas(SqlConnection connection, int CodCliente, AfiBeneSocioCreceDto beneficio)
        {
            if (beneficio.monto_primera_tarjeta > 0)
            {
                GestionarTarjetasConMonto(connection, CodCliente, beneficio);
            }
            else
            {
                LimpiarTarjetas(connection, beneficio);
            }
        }

        /// <summary>
        /// Reasigna el producto de tarjeta y ajusta montos cuando hay monto de primera tarjeta.
        /// </summary>
        private void GestionarTarjetasConMonto(SqlConnection connection, int CodCliente, AfiBeneSocioCreceDto beneficio)
        {
            var llaves = new { codBeneficio = beneficio.cod_beneficio, consec = beneficio.consec };

            connection.Execute(
                "DELETE FROM afi_bene_prodasg WHERE cod_beneficio = @codBeneficio AND consec = @consec", llaves);

            var codProducto = _config.GetSection("AFI_Beneficios").GetSection("CodProductoCrece").Value ?? string.Empty;
            var codTarjeta = connection.QueryFirstOrDefault<string>(
                "SELECT VALOR FROM [SIF_PARAMETROS] WHERE COD_PARAMETRO = @codProducto", new { codProducto });

            connection.Execute(
                @"INSERT afi_bene_prodasg(consec,cod_beneficio,cod_producto,cantidad,costo_unidad,REGISTRO_FECHA,REGISTRO_USUARIO)
                  VALUES(@consec,@codBeneficio,@codTarjeta,1,@monto,GETDATE(),@usuario)",
                new
                {
                    consec = beneficio.consec,
                    codBeneficio = beneficio.cod_beneficio,
                    codTarjeta,
                    monto = beneficio.monto_primera_tarjeta,
                    usuario = beneficio.modifica_usuario
                });

            if (beneficio.entrega_primera_tarjeta && beneficio.monto_segunda_tarjeta > 0)
            {
                connection.Execute(
                    "UPDATE afi_bene_prodasg SET cantidad = 2 WHERE consec = @consec AND cod_beneficio = @codBeneficio", llaves);
            }

            ActualizarMontosSiCambio(connection, CodCliente, beneficio);
        }

        /// <summary>
        /// Compara el monto registrado contra el nuevo total y, si cambió, actualiza montos y deja bitácora.
        /// </summary>
        private void ActualizarMontosSiCambio(SqlConnection connection, int CodCliente, AfiBeneSocioCreceDto beneficio)
        {
            var llaves = new { codBeneficio = beneficio.cod_beneficio, consec = beneficio.consec };

            var monto = connection.QueryFirstOrDefault<float>(
                "SELECT MONTO_NUEVO FROM AFI_BENE_REGISTRO_MONTOS WHERE CONSEC = @consec AND COD_BENEFICIO = @codBeneficio", llaves);

            var montoNuevo = connection.QueryFirstOrDefault<float>(
                "SELECT SUM(MONTO_PRIMERA_TARJETA + MONTO_SEGUNDA_TARJETA) FROM AFI_BENE_SOCIO_CRECE WHERE CONSEC = @consec AND COD_BENEFICIO = @codBeneficio", llaves);

            const float epsilon = 0.0001f;
            if (Math.Abs(monto - montoNuevo) <= epsilon)
            {
                return;
            }

            connection.Execute(
                @"UPDATE [dbo].[AFI_BENE_REGISTRO_MONTOS]
                     SET [MONTO_NUEVO]=@montoNuevo,[MONTO_ANTERIOR]=@monto,[NOTAS]=@notas,
                         [REGISTRO_FECHA]=GETDATE(),[REGISTRO_USUARIO]=@usuario
                   WHERE CONSEC=@consec AND [COD_BENEFICIO]=@codBeneficio",
                new
                {
                    montoNuevo,
                    monto,
                    notas = beneficio.observaciones_bene,
                    usuario = beneficio.modifica_usuario,
                    consec = beneficio.consec,
                    codBeneficio = beneficio.cod_beneficio
                });

            connection.Execute(
                "UPDATE AFI_BENE_OTORGA SET MONTO_APLICADO = @montoNuevo WHERE CONSEC = @consec AND [COD_BENEFICIO] = @codBeneficio",
                new { montoNuevo, consec = beneficio.consec, codBeneficio = beneficio.cod_beneficio });

            _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDto
            {
                EmpresaId = CodCliente,
                cod_beneficio = beneficio.cod_beneficio,
                consec = beneficio.consec,
                movimiento = "Actualiza",
                detalle = $"Actualiza Monto de {monto} a {montoNuevo} ",
                registro_usuario = beneficio.modifica_usuario ?? string.Empty
            });
        }

        /// <summary>
        /// Elimina el producto asignado y pone en cero el monto aplicado cuando no hay monto de primera tarjeta.
        /// </summary>
        private static void LimpiarTarjetas(SqlConnection connection, AfiBeneSocioCreceDto beneficio)
        {
            var llaves = new { codBeneficio = beneficio.cod_beneficio, consec = beneficio.consec };

            var existe = connection.QueryFirstOrDefault<int>(
                "SELECT COUNT(*) FROM afi_bene_prodasg WHERE cod_beneficio = @codBeneficio AND consec = @consec", llaves);

            if (existe <= 0)
            {
                return;
            }

            connection.Execute(
                "DELETE FROM afi_bene_prodasg WHERE cod_beneficio = @codBeneficio AND consec = @consec", llaves);
            connection.Execute(
                "UPDATE AFI_BENE_OTORGA SET MONTO_APLICADO = 0 WHERE CONSEC = @consec AND [COD_BENEFICIO] = @codBeneficio", llaves);
        }

        // ==================== Sesiones Crece ====================

        /// <summary>
        /// Guarda una sesión Crece: inserta (id_sesion 0) o actualiza.
        /// </summary>
        public ErrorDto BeneSocioCreceSesion_Guardar(int CodCliente, AfiBeneSocioCreceSesionesDto beneficio)
        {
            try
            {
                return beneficio.id_sesion > 0
                    ? BeneSocioCreceSession_Actualizar(CodCliente, beneficio)
                    : BeneSocioCreceSession_Insertar(CodCliente, beneficio);
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = "BeneSocioCreceSesion_Guardar - " + ex.Message };
            }
        }

        /// <summary>
        /// Inserta una nueva sesión Crece y devuelve el id generado en Description.
        /// </summary>
        private ErrorDto BeneSocioCreceSession_Insertar(int CodCliente, AfiBeneSocioCreceSesionesDto beneficio)
        {
            const string sqlInsert = @"
                INSERT INTO [dbo].[AFI_BENE_SOCIO_CRECE_SESIONES]
                    ([COD_BENEFICIO],[CONSEC],[SESION],[ASISTENCIA],[TAREA],[NOTAS],[SESION_FECHA],[REGISTRO_FECHA],[REGSITRO_USUARIO])
                VALUES
                    (@codBeneficio,@consec,@sesion,@asistencia,@tarea,@notas,@sesionFecha,GETDATE(),@registroUsuario)";

            const string sqlId = "SELECT IDENT_CURRENT('AFI_BENE_SOCIO_CRECE_SESIONES') AS id";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                connection.Execute(sqlInsert, new
                {
                    codBeneficio = beneficio.cod_beneficio,
                    consec = beneficio.consec,
                    sesion = beneficio.sesion,
                    asistencia = beneficio.asistencia ? 1 : 0,
                    tarea = beneficio.tarea ? 1 : 0,
                    notas = beneficio.notas,
                    sesionFecha = beneficio.sesion_fecha,
                    registroUsuario = beneficio.regsitro_usuario
                });

                return connection.QueryFirstOrDefault<int>(sqlId);
            });

            return new ErrorDto
            {
                Code = result.Code,
                Description = result.Code == 0 ? result.Result.ToString() : "BeneSocioCreceSession_Insertar - " + result.Description
            };
        }

        /// <summary>
        /// Actualiza una sesión Crece existente.
        /// </summary>
        private ErrorDto BeneSocioCreceSession_Actualizar(int CodCliente, AfiBeneSocioCreceSesionesDto beneficio)
        {
            const string sqlUpdate = @"
                UPDATE [dbo].[AFI_BENE_SOCIO_CRECE_SESIONES]
                   SET [SESION]=@sesion,[ASISTENCIA]=@asistencia,[TAREA]=@tarea,[NOTAS]=@notas,
                       [SESION_FECHA]=@sesionFecha,[REGISTRO_FECHA]=GETDATE(),[REGSITRO_USUARIO]=@registroUsuario
                 WHERE ID_SESION=@idSesion";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Execute(sqlUpdate, new
                {
                    sesion = beneficio.sesion,
                    asistencia = beneficio.asistencia ? 1 : 0,
                    tarea = beneficio.tarea ? 1 : 0,
                    notas = beneficio.notas,
                    sesionFecha = beneficio.sesion_fecha,
                    registroUsuario = beneficio.regsitro_usuario,
                    idSesion = beneficio.id_sesion
                }));

            return new ErrorDto
            {
                Code = result.Code,
                Description = result.Code == 0 ? beneficio.id_sesion.ToString() : "BeneSocioCreceSession_Actualizar - " + result.Description
            };
        }
    }
}
