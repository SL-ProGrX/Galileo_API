using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrCatalogoRequisitosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _Bitacora;
        private readonly int vModulo = 3;

        public FrmCrCatalogoRequisitosDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmCrCatalogoRequisitosDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            _Bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene el catalogo de requisitos 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrRequisitosData>> CrCatalogoRequisitos_Obtener(int codEmpresa)
        {
            string query = @"select cod_requisito,descripcion,visible 
            from requisitos_adicionales order by cod_requisito";
            return DbHelper.ExecuteListQuery<CrRequisitosData>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene los tipos de catalogos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="nivel"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrCatalogosTipos_Obtener(int codEmpresa, string nivel)
        {
            string query = "";
            if (nivel == "L")
            {
                query = @"select codigo as item,descripcion from catalogo where retencion = 'N' and poliza = 'N' 
                    and requisitos_tipo = 'L' order by codigo";
            }
            else
            {
                query = @"select garantia as item,descripcion from Crd_Garantia_Tipos order by Garantia";
            }
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene los requisitos asignados a un catalogo especifico
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="nivel"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<CrRequisitosData>> CrRequisitos_Asignados_Obtener(int codEmpresa, string nivel, string codigo)
        {
            string query = "";
            if (nivel == "L")
            {
                query = @"select R.*,isnull(A.opcional,0) as 'opcionalX', 
                CASE 
                    WHEN A.codigo IS NOT NULL THEN 1
                    ELSE 0
                END AS Existe 
                from Requisitos_Adicionales R left Join Requisitos_asignacion A 
                on R.cod_requisito = A.cod_requisito and A.codigo = @codigo 
                order by existe desc,R.cod_requisito";
            }
            else
            {
                query = @"select R.*,isnull(A.opcional,0) as 'opcionalX', 
                CASE 
                    WHEN A.Garantia  IS NOT NULL THEN 1
                    ELSE 0
                END AS Existe 
                from Requisitos_Adicionales R left Join CRD_GARANTIA_REQUISITOS A 
                on R.cod_requisito = A.cod_requisito and A.Garantia = @codigo  
                order by existe desc,R.cod_requisito";
            }
            return DbHelper.ExecuteListQuery<CrRequisitosData>(_portalDb, codEmpresa, query, new { codigo });
        }

        /// <summary>
        /// Guarda un requisito en el catalogo
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoRequisitos_Guardar(int codEmpresa, string usuario, CrRequisitosData request)
        {
            var existe = ExisteRequisito(codEmpresa, request.cod_requisito);

            var resp = existe
                ? ActualizarRequisito(codEmpresa, usuario, request)
                : InsertarRequisito(codEmpresa, usuario, request);

            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = "Información guardada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Elimina un requisito del catalogo
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoRequisitos_Eliminar(int codEmpresa, string codigo, string usuario)
        {
            const string sqlDelete = @"DELETE FROM requisitos_adicionales 
            WHERE cod_requisito = @Codigo;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    Codigo = codigo.Trim()
                }
            );
            if (respDelete.Code < 0)
                return respDelete;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Elimina - WEB",
                detalle: $"Requisito Adicional Cod: {codigo}"
            );
            return respDelete;
        }

        /// <summary>
        /// Asigna o desasigna un requisito a un catalogo especifico
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoRequisitos_Asignar(int codEmpresa, CrRequisitoAsignacionRequest request)
        {
            if (request.nivel == "L")
            {
                return ProcesarRequisitoLinea(codEmpresa, request);
            }

            if (request.nivel == "G")
            {
                return ProcesarRequisitoGarantia(codEmpresa, request);
            }

            return new ErrorDto
            {
                Code = -1,
                Description = "El nivel de aplicacion no es valido."
            };
        }

        /// <summary>
        /// Actualiza requisito
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto ActualizarRequisito(int codEmpresa, string usuario, CrRequisitosData request)
        {
            const string sqlUpdate = @"
            UPDATE requisitos_adicionales
            SET
                descripcion = @Descripcion,
                visible     = @Visible
            WHERE cod_requisito = @CodRequisito;";

            var respUpdate = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    CodRequisito = request.cod_requisito,
                    Descripcion = request.descripcion,
                    Visible = request.visible ? 1 : 0
                }
            );

            if (respUpdate.Code < 0)
                return respUpdate;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Modifica - WEB",
                detalle: $"Requisito Adicional Cod: {request.cod_requisito}"
            );

            return respUpdate;
        }

        /// <summary>
        /// Inserta requisito
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto InsertarRequisito(int codEmpresa, string usuario, CrRequisitosData request)
        {
            const string sqlInsert = @"
            INSERT INTO requisitos_adicionales
            (
                cod_requisito,
                descripcion,
                visible
            )
            VALUES
            (
                @CodRequisito,
                @Descripcion,
                @Visible
            );";

            var respInsert = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    CodRequisito = request.cod_requisito,
                    Descripcion = request.descripcion,
                    Visible = request.visible ? 1 : 0
                }
            );

            if (respInsert.Code < 0)
                return respInsert;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Registra - WEB",
                detalle: $"Requisito Adicional Cod: {request.cod_requisito}"
            );

            return respInsert;
        }

        /// <summary>
        /// Valida existencia de requisito
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codRequisito"></param>
        /// <returns></returns>
        private bool ExisteRequisito(int codEmpresa, string codRequisito)
        {
            const string sqlExiste = @"SELECT ISNULL(COUNT(*), 0) 
            FROM requisitos_adicionales WHERE cod_requisito = @CodRequisito;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlExiste,
                0,
                new
                {
                    CodRequisito = codRequisito.Trim()
                }
            );

            return resp.Result > 0;
        }

        /// <summary>
        /// Procesa requisito por linea, valida si requiere asignar o desasignar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto ProcesarRequisitoLinea(int codEmpresa, CrRequisitoAsignacionRequest request)
        {
            ErrorDto resp;

            if (request.columna == 4)
            {
                resp = AsignarRequisitoLinea(
                    codEmpresa,
                    request.codigo,
                    request.codRequisito,
                    request.opcional,
                    request.isChecked
                );

                if (resp.Code >= 0)
                {
                    RegistrarBitacora(
                        codEmpresa,
                        request.usuario,
                        movimiento: request.isChecked ? "Registra - WEB" : "Borrar - WEB",
                        detalle: $"Requisito : {request.codRequisito} a la Línea: {request.codigo}"
                    );
                }

                return resp;
            }

            if (request.columna == 3)
            {
                if (!request.isChecked)
                {
                    return new ErrorDto
                    {
                        Code = 0,
                        Description = "No aplica actualizar opcional porque el requisito no está asignado."
                    };
                }

                resp = ActualizarOpcionalLinea(
                    codEmpresa,
                    request.codigo,
                    request.codRequisito,
                    request.opcional
                );

                if (resp.Code >= 0)
                {
                    RegistrarBitacora(
                        codEmpresa,
                        request.usuario,
                        movimiento: "Modifica - WEB",
                        detalle: $"Requisito : {request.codRequisito} a la Línea: {request.codigo}"
                    );
                }

                return resp;
            }

            return new ErrorDto
            {
                Code = -1,
                Description = "La columna enviada no es válida."
            };
        }

        /// <summary>
        /// Procesa requisito por garantia, valida si requiere asignar o desasignar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto ProcesarRequisitoGarantia(int codEmpresa, CrRequisitoAsignacionRequest request)
        {
            ErrorDto resp;

            if (request.columna == 4)
            {
                resp = AsignarRequisitoGarantia(
                    codEmpresa,
                    request.codigo,
                    request.codRequisito,
                    request.opcional,
                    request.isChecked
                );

                if (resp.Code >= 0)
                {
                    RegistrarBitacora(
                        codEmpresa,
                        request.usuario,
                        movimiento: request.isChecked ? "Registra - WEB" : "Borrar - WEB",
                        detalle: $"Requisito : {request.codRequisito} a la Garantía: {request.codigo}"
                    );
                }

                return resp;
            }

            if (request.columna == 3)
            {
                if (!request.isChecked)
                {
                    return new ErrorDto
                    {
                        Code = 0,
                        Description = "No aplica actualizar opcional porque el requisito no está asignado."
                    };
                }

                resp = ActualizarOpcionalGarantia(
                    codEmpresa,
                    request.codigo,
                    request.codRequisito,
                    request.opcional
                );

                if (resp.Code >= 0)
                {
                    RegistrarBitacora(
                        codEmpresa,
                        request.usuario,
                        movimiento: "Modifica - WEB",
                        detalle: $"Requisito : {request.codRequisito} a la Garantía: {request.codigo}"
                    );
                }

                return resp;
            }

            return new ErrorDto
            {
                Code = -1,
                Description = "La columna enviada no es válida."
            };
        }

        /// <summary>
        /// Asigna requisito a catalogo por linea
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <param name="codRequisito"></param>
        /// <param name="opcional"></param>
        /// <param name="isChecked"></param>
        /// <returns></returns>
        private ErrorDto AsignarRequisitoLinea(int codEmpresa, string codigo, string codRequisito, bool opcional, bool isChecked)
        {
            codigo = codigo?.Trim() ?? string.Empty;
            codRequisito = codRequisito?.Trim() ?? string.Empty;

            if (isChecked)
            {
                const string sqlInsert = @"
                IF NOT EXISTS (
                    SELECT 1
                    FROM requisitos_asignacion
                    WHERE codigo = @Codigo
                      AND cod_requisito = @CodRequisito
                )
                BEGIN
                    INSERT INTO requisitos_asignacion (codigo, cod_requisito, opcional)
                    VALUES (@Codigo, @CodRequisito, @Opcional)
                END";

                return DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    sqlInsert,
                    new
                    {
                        Codigo = codigo,
                        CodRequisito = codRequisito,
                        Opcional = opcional ? 1 : 0
                    }
                );
            }

            const string sqlDelete = @"
            DELETE FROM requisitos_asignacion
            WHERE codigo = @Codigo
              AND cod_requisito = @CodRequisito;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    Codigo = codigo,
                    CodRequisito = codRequisito
                }
            );
        }

        /// <summary>
        /// Asigna requisito a catalago por garantia
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="garantia"></param>
        /// <param name="codRequisito"></param>
        /// <param name="opcional"></param>
        /// <param name="isChecked"></param>
        /// <returns></returns>
        private ErrorDto AsignarRequisitoGarantia(int codEmpresa, string garantia, string codRequisito, bool opcional, bool isChecked)
        {
            garantia = garantia?.Trim() ?? string.Empty;
            codRequisito = codRequisito?.Trim() ?? string.Empty;

            if (isChecked)
            {
                const string sqlInsert = @"
                IF NOT EXISTS (
                    SELECT 1
                    FROM CRD_GARANTIA_REQUISITOS
                    WHERE garantia = @Garantia
                      AND cod_requisito = @CodRequisito
                )
                BEGIN
                    INSERT INTO CRD_GARANTIA_REQUISITOS (garantia, cod_requisito, opcional)
                    VALUES (@Garantia, @CodRequisito, @Opcional)
                END";

                return DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    sqlInsert,
                    new
                    {
                        Garantia = garantia,
                        CodRequisito = codRequisito,
                        Opcional = opcional ? 1 : 0
                    }
                );
            }

            const string sqlDelete = @"
            DELETE FROM CRD_GARANTIA_REQUISITOS
            WHERE garantia = @Garantia
              AND cod_requisito = @CodRequisito;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    Garantia = garantia,
                    CodRequisito = codRequisito
                }
            );
        }

        /// <summary>
        /// Actualiza opcional de requisito por linea
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <param name="codRequisito"></param>
        /// <param name="opcional"></param>
        /// <returns></returns>
        private ErrorDto ActualizarOpcionalLinea(int codEmpresa, string codigo, string codRequisito, bool opcional)
        {
            const string sqlUpdate = @"
            UPDATE requisitos_asignacion
            SET opcional = @Opcional
            WHERE codigo = @Codigo
              AND cod_requisito = @CodRequisito;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    Codigo = codigo.Trim(),
                    CodRequisito = codRequisito.Trim(),
                    Opcional = opcional ? 1 : 0
                }
            );
        }

        /// <summary>
        /// Actualiza opcional de requisito por garantia
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="garantia"></param>
        /// <param name="codRequisito"></param>
        /// <param name="opcional"></param>
        /// <returns></returns>
        private ErrorDto ActualizarOpcionalGarantia(int codEmpresa, string garantia, string codRequisito, bool opcional)
        {
            const string sqlUpdate = @"
            UPDATE CRD_GARANTIA_REQUISITOS
            SET opcional = @Opcional
            WHERE garantia = @Garantia
              AND cod_requisito = @CodRequisito;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    Garantia = garantia.Trim(),
                    CodRequisito = codRequisito.Trim(),
                    Opcional = opcional ? 1 : 0
                }
            );
        }

        /// <summary>
        /// Registra en bitacora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="movimiento"></param>
        /// <param name="detalle"></param>
        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _Bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
