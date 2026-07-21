using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo.Models.GA;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralReqDB
    {
        /// <summary>
        /// Registra un requisito del beneficio (los documentos se gestionan en GA) y deja traza en bitácora.
        /// </summary>
        /// <param name="requisito">Datos del requisito a registrar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BeneRegistroRequisitos_Guardar(BeneRequisitosGuardar requisito)
        {
            const string sqlInsert = @"
                INSERT INTO AFI_BENE_REGISTRO_REQUISITOS
                    ([COD_BENEFICIO],[CONSEC],[COD_REQUISITO],[REGISTRO_FECHA],[REGISTRO_USUARIO])
                VALUES
                    (@codBeneficio,@consec,@codRequisito,GETDATE(),@usuario)";

            var result = DbHelper.WithConn(CreatePortalDb(), requisito.codCliente, connection =>
                connection.Execute(sqlInsert, new
                {
                    codBeneficio = requisito.cod_beneficio,
                    consec = requisito.consec,
                    codRequisito = requisito.cod_requisito,
                    usuario = requisito.usuario
                }));

            if (result.Code != 0)
            {
                return new ErrorDto { Code = -1, Description = "BeneRegistroRequisitos_Guardar: " + result.Description };
            }

            RegistrarBitacora(requisito.codCliente, requisito.cod_beneficio, requisito.consec, requisito.usuario,
                $"Se cargo Requisito COD: [{requisito.cod_requisito}]");

            return new ErrorDto { Code = result.Code, Description = result.Description };
        }

        /// <summary>
        /// Elimina un requisito del beneficio y deja traza en bitácora.
        /// </summary>
        /// <param name="CodCliente">Código de empresa/cliente.</param>
        /// <param name="cod_beneficio">Código del beneficio.</param>
        /// <param name="consec">Consecutivo del beneficio.</param>
        /// <param name="cod_requisito">Código del requisito a eliminar.</param>
        /// <param name="usuario">Usuario que ejecuta la acción.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BeneRegistroRequisitos_Eliminar(int CodCliente, string cod_beneficio, int consec, string cod_requisito, string usuario)
        {
            if (cod_requisito == null)
            {
                return new ErrorDto { Code = 0, Description = string.Empty };
            }

            const string sqlDelete = @"
                DELETE [dbo].[AFI_BENE_REGISTRO_REQUISITOS]
                 WHERE COD_REQUISITO = @codRequisito AND CONSEC = @consec AND COD_BENEFICIO = @codBeneficio";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Execute(sqlDelete, new { codRequisito = cod_requisito, consec, codBeneficio = cod_beneficio }));

            if (result.Code != 0)
            {
                return new ErrorDto { Code = -1, Description = "BeneRegistroRequisitos_Eliminar: " + result.Description };
            }

            RegistrarBitacora(CodCliente, cod_beneficio, consec, usuario,
                $"Se elimina Requisito COD: [{cod_requisito}]");

            return new ErrorDto { Code = result.Code, Description = result.Description };
        }

        /// <summary>
        /// Asocia el archivo (GA_Files) a un requisito del beneficio, reasignando llaves y registrando el requisito.
        /// </summary>
        /// <param name="modulo">Módulo destino en GA.</param>
        /// <param name="TypeId">Tipo de documento destino en GA.</param>
        /// <param name="requisito">Requisito serializado en JSON (BeneRequisitosGuardar).</param>
        /// <param name="data">Llaves del archivo en GA.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BeneRegistroRequisito_Asociar(string modulo, string TypeId, string requisito, DocumentosArchivoDto data)
        {
            var resp = new ErrorDto();
            try
            {
                var beneRequisitos = JsonConvert.DeserializeObject<BeneRequisitosGuardar>(requisito) ?? new BeneRequisitosGuardar();
                var gaConnString = _config.GetConnectionString("GAConnString");

                using var connection = new SqlConnection(gaConnString);

                var existe = connection.QueryFirstOrDefault<int>(
                    @"SELECT COUNT(*) FROM GA_Files
                       WHERE Llave_01 = @llave01 AND Llave_02 = @llave02 AND Llave_03 = @codRequisito",
                    new { llave01 = data.llave_01, llave02 = data.llave_02, codRequisito = beneRequisitos.cod_requisito });

                if (existe > 0)
                {
                    resp.Code = connection.Execute(
                        @"UPDATE GA_Files SET TypeId = '999', ModuloId = 'CL_01', Llave_03 = @llave03
                           WHERE Llave_01 = @llave01 AND Llave_02 = @llave02 AND Llave_03 = @codRequisito",
                        new { llave03 = data.llave_03, llave01 = data.llave_01, llave02 = data.llave_02, codRequisito = beneRequisitos.cod_requisito });

                    BeneRegistroRequisitos_Eliminar(beneRequisitos.codCliente, beneRequisitos.cod_beneficio,
                        beneRequisitos.consec, beneRequisitos.cod_requisito, beneRequisitos.usuario);
                }

                resp.Code = connection.Execute(
                    @"UPDATE GA_Files SET TypeId = @typeId, ModuloId = @modulo, Llave_03 = @codRequisito
                       WHERE Llave_01 = @llave01 AND Llave_02 = @llave02 AND Llave_03 = @llave03",
                    new { typeId = TypeId, modulo, codRequisito = beneRequisitos.cod_requisito, llave01 = data.llave_01, llave02 = data.llave_02, llave03 = data.llave_03 });

                BeneRegistroRequisitos_Guardar(beneRequisitos);
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "BeneRegistroRequisito_Asociar: " + ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Registra un movimiento del requisito en la bitácora de beneficios (helper compartido).
        /// </summary>
        private void RegistrarBitacora(int codCliente, string codBeneficio, int consec, string usuario, string detalle)
        {
            _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDto
            {
                EmpresaId = codCliente,
                cod_beneficio = codBeneficio,
                consec = consec,
                movimiento = "Actualiza",
                detalle = detalle,
                registro_usuario = usuario
            });
        }
    }
}
