using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrCatalogoRequisitosDb
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 3;

        private const string NivelLinea = "L";
        private const string NivelGarantia = "G";

        public FrmCrCatalogoRequisitosDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmCrCatalogoRequisitosDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDB = portalDB;
            _bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene el catalogo de requisitos 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrRequisitosData>> CrCatalogoRequisitos_Obtener(int codEmpresa)
        {
            string sqlQuery = @"select cod_requisito,descripcion,visible 
            from requisitos_adicionales order by cod_requisito";
            return DbHelper.ExecuteListQuery<CrRequisitosData>(_portalDB, codEmpresa, sqlQuery);
        }

        /// <summary>
        /// Obtiene los tipos de catalogos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="nivel"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrCatalogosTipos_Obtener(int codEmpresa, string nivel)
        {
            string sqlQuery = nivel == NivelLinea
                ? @"select codigo as item,descripcion from catalogo where retencion = 'N' and poliza = 'N' 
                    and requisitos_tipo = 'L' order by codigo"
                : @"select garantia as item,descripcion from Crd_Garantia_Tipos order by Garantia";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDB, codEmpresa, sqlQuery);
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
            string sqlQuery = nivel == NivelLinea
                ? @"select R.*,isnull(A.opcional,0) as 'opcionalX', 
                CASE 
                    WHEN A.codigo IS NOT NULL THEN 1
                    ELSE 0
                END AS Existe 
                from Requisitos_Adicionales R left Join Requisitos_asignacion A 
                on R.cod_requisito = A.cod_requisito and A.codigo = @codigo 
                order by existe desc,R.cod_requisito"
                : @"select R.*,isnull(A.opcional,0) as 'opcionalX', 
                CASE 
                    WHEN A.Garantia IS NOT NULL THEN 1
                    ELSE 0
                END AS Existe 
                from Requisitos_Adicionales R left Join CRD_GARANTIA_REQUISITOS A 
                on R.cod_requisito = A.cod_requisito and A.Garantia = @codigo  
                order by existe desc,R.cod_requisito";

            return DbHelper.ExecuteListQuery<CrRequisitosData>(_portalDB, codEmpresa, sqlQuery, new { codigo });
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
                _portalDB,
                codEmpresa,
                sqlDelete,
                new
                {
                    Codigo = codigo.Trim()
                });

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
            return request.nivel switch
            {
                NivelLinea => ProcesarRequisito(codEmpresa, request, ObtenerConfigNivel(request.nivel)),
                NivelGarantia => ProcesarRequisito(codEmpresa, request, ObtenerConfigNivel(request.nivel)),
                _ => new ErrorDto
                {
                    Code = -1,
                    Description = "El nivel de aplicacion no es valido."
                }
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
                _portalDB,
                codEmpresa,
                sqlUpdate,
                new
                {
                    CodRequisito = request.cod_requisito,
                    Descripcion = request.descripcion,
                    Visible = request.visible ? 1 : 0
                });

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
                _portalDB,
                codEmpresa,
                sqlInsert,
                new
                {
                    CodRequisito = request.cod_requisito,
                    Descripcion = request.descripcion,
                    Visible = request.visible ? 1 : 0
                });

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
        /// Verifica si un requisito existe en el catalogo
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codRequisito"></param>
        /// <returns></returns>
        private bool ExisteRequisito(int codEmpresa, string codRequisito)
        {
            const string sqlExiste = @"SELECT ISNULL(COUNT(*), 0) 
            FROM requisitos_adicionales WHERE cod_requisito = @CodRequisito;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDB,
                codEmpresa,
                sqlExiste,
                0,
                new
                {
                    CodRequisito = codRequisito.Trim()
                });

            return resp.Result > 0;
        }

        /// <summary>
        /// Procesa requisito
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private ErrorDto ProcesarRequisito(int codEmpresa, CrRequisitoAsignacionRequest request, RequisitoNivelConfig config)
        {
            ErrorDto resp;

            if (request.columna == 4)
            {
                resp = AsignarRequisito(
                    codEmpresa,
                    request.codigo,
                    request.codRequisito,
                    request.opcional,
                    request.isChecked,
                    config
                );

                if (resp.Code >= 0)
                {
                    RegistrarBitacora(
                        codEmpresa,
                        request.usuario,
                        movimiento: request.isChecked ? "Registra - WEB" : "Borrar - WEB",
                        detalle: $"Requisito : {request.codRequisito} a la {config.DescripcionBitacora}: {request.codigo}"
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

                resp = ActualizarOpcional(
                    codEmpresa,
                    request.codigo,
                    request.codRequisito,
                    request.opcional,
                    config
                );

                if (resp.Code >= 0)
                {
                    RegistrarBitacora(
                        codEmpresa,
                        request.usuario,
                        movimiento: "Modifica - WEB",
                        detalle: $"Requisito : {request.codRequisito} a la {config.DescripcionBitacora}: {request.codigo}"
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
        /// Asigna requisito
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="valorCatalogo"></param>
        /// <param name="codRequisito"></param>
        /// <param name="opcional"></param>
        /// <param name="isChecked"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private ErrorDto AsignarRequisito(
            int codEmpresa,
            string valorCatalogo,
            string codRequisito,
            bool opcional,
            bool isChecked,
            RequisitoNivelConfig config)
        {
            valorCatalogo = valorCatalogo?.Trim() ?? string.Empty;
            codRequisito = codRequisito?.Trim() ?? string.Empty;

            if (isChecked)
            {
                string sqlInsert = $@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM {config.Tabla}
                    WHERE {config.CampoCatalogo} = @ValorCatalogo
                      AND cod_requisito = @CodRequisito
                )
                BEGIN
                    INSERT INTO {config.Tabla} ({config.CampoCatalogo}, cod_requisito, opcional)
                    VALUES (@ValorCatalogo, @CodRequisito, @Opcional)
                END";

                return DbHelper.ExecuteNonQuery(
                    _portalDB,
                    codEmpresa,
                    sqlInsert,
                    new
                    {
                        ValorCatalogo = valorCatalogo,
                        CodRequisito = codRequisito,
                        Opcional = opcional ? 1 : 0
                    });
            }

            string sqlDelete = $@"
            DELETE FROM {config.Tabla}
            WHERE {config.CampoCatalogo} = @ValorCatalogo
              AND cod_requisito = @CodRequisito;";

            return DbHelper.ExecuteNonQuery(
                _portalDB,
                codEmpresa,
                sqlDelete,
                new
                {
                    ValorCatalogo = valorCatalogo,
                    CodRequisito = codRequisito
                });
        }

        /// <summary>
        /// Actualiza opcional de requisito
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="valorCatalogo"></param>
        /// <param name="codRequisito"></param>
        /// <param name="opcional"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private ErrorDto ActualizarOpcional(
            int codEmpresa,
            string valorCatalogo,
            string codRequisito,
            bool opcional,
            RequisitoNivelConfig config)
        {
            const string parametroCatalogo = "ValorCatalogo";

            string sqlUpdate = $@"
            UPDATE {config.Tabla}
            SET opcional = @Opcional
            WHERE {config.CampoCatalogo} = @{parametroCatalogo}
              AND cod_requisito = @CodRequisito;";

            return DbHelper.ExecuteNonQuery(
                _portalDB,
                codEmpresa,
                sqlUpdate,
                new
                {
                    ValorCatalogo = valorCatalogo.Trim(),
                    CodRequisito = codRequisito.Trim(),
                    Opcional = opcional ? 1 : 0
                });
        }

        /// <summary>
        /// Obtiene nivel de aplicacion
        /// </summary>
        /// <param name="nivel"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static RequisitoNivelConfig ObtenerConfigNivel(string nivel)
        {
            return nivel switch
            {
                NivelLinea => new RequisitoNivelConfig("requisitos_asignacion", "codigo", "Línea"),
                NivelGarantia => new RequisitoNivelConfig("CRD_GARANTIA_REQUISITOS", "garantia", "Garantía"),
                _ => throw new ArgumentException("Nivel no válido.", nameof(nivel))
            };
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
            _bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
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