namespace Galileo.Models.AF
{
    public class BeneCategoria
    {
        public string cod_categoria { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool i_apremiante { get; set; }
        public bool i_reconocimientos { get; set; }
        public bool i_crece { get; set; }
        public bool i_fena { get; set; }
        public bool i_sepelio { get; set; }
        public bool i_desastres { get; set; }
        public bool activo { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }

    }

    public class BEeneCategoriaDataLista
    {
        public int Total { get; set; }
        public List<BeneCategoria> Lista { get; set; } = new List<BeneCategoria>();
    }

    public class BeneCategoriaPermisos
    {
        public string nombre { get; set; } = string.Empty;
        public bool i_cambiar_estado { get; set; } = false;
        public bool i_modifica_expediente { get; set; } = false;
        public bool i_traslado_tesoreria { get; set; } = false;
        public bool i_pago_programar { get; set; } = false;
        public bool i_pago_aprobar_m { get; set; } = false;
        public bool i_pago_realizar { get; set; } = false;
        public bool i_ingresar_solicitud { get; set; } = false;
        public bool i_periodo { get; set; } = false;
        public bool i_pago_consulta { get; set; } = false;
        public bool i_aprobar { get; set; } = false;
        public bool i_rechazar { get; set; } = false;
        public bool i_anular { get; set; } = false;
        public bool i_devolver_resolucion { get; set; } = false;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public int cod_rol { get; set; }
    }

    public class  BeneValidaLista
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class BeneCategoriaValidaLista
    {
        public string cod_categoria { get; set; } = string.Empty;
        public int cod_val { get; set; }
        public bool registro { get; set; } = false;
        public bool registro_justifica { get; set; } = false;
        public bool registro_info { get; set; } = false;
        public bool pago { get; set; } = false;
        public bool pago_justifica { get; set; } = false;
        public bool pago_info { get; set; } = false;
        public bool estado { get; set; } = false;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string? modifica_usuario { get; set; } = string.Empty;
        public Nullable<DateTime> modifica_fecha { get; set; }
    }

    public class AfiBeneCalidaciones
    {
        public int cod_val { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string query_val { get; set; } = string.Empty;
        public string msj_val { get; set; } = string.Empty;
        public int resultado_val { get; set; } = 0;
        public bool registro_justifica { get; set; } = false;
        public bool pago_justifica { get; set; } = false;
    }

    public static class QuerysStringValidaciones
    {
        public const string registroVal = "REGISTRO";
        public const string pagoVal = "PAGO";

        public const string CodCategoriaPlaceholder = "@cod_categoria";
        public const string CodBeneficioPlaceholder = "@cod_beneficio";
        public const string CedulaPlaceholder = "@cedula";
        public const string UsuarioPlaceholder = "@usuario";
        public const string IdBeneficioPlaceholder = "@id_beneficio";
        public const string MontoUsuarioPlaceholder = "@monto_usuario";
        public const string SepelioIdentificacionPlaceholder = "@sepelio_identificacion";

        public const string registroP = @"SELECT * 
                              FROM AFI_BENE_VALIDACIONES 
                              WHERE ESTADO = 1 AND 
                              TIPO = 'P' AND 
                              REGISTRO = 1 
                              ORDER BY PRIORIDAD ASC";

        public const string registroPCategoria = @"
                        select abv.* 
                        FROM AFI_BENE_VALIDA_CATEGORIA c 
                        left join AFI_BENE_VALIDACIONES abv ON abv.COD_VAL = c.COD_VAL
                        WHERE COD_CATEGORIA = 
                        (
	                        SELECT ab.COD_CATEGORIA 
                            FROM AFI_BENEFICIOS ab 
	                        WHERE ab.COD_BENEFICIO = @cod_beneficio
                        ) 
                        AND c.ESTADO = 1 
                        AND TIPO = 'P' 
                        AND REGISTRO = 1 
                        order by abv.PRIORIDAD asc";

        public const string pagoP = @"SELECT * 
                              FROM AFI_BENE_VALIDACIONES 
                              WHERE ESTADO = 1 
                                AND PAGO = 1 
                                AND TIPO = 'P' 
                              ORDER BY PRIORIDAD ASC";

        public const string pagoPCategoria = @"
                        select abv.* 
                        FROM AFI_BENE_VALIDA_CATEGORIA c 
                        left join AFI_BENE_VALIDACIONES abv ON abv.COD_VAL = c.COD_VAL
                        WHERE COD_CATEGORIA = 
                        (
	                        SELECT ab.COD_CATEGORIA 
                            FROM AFI_BENEFICIOS ab 
	                        WHERE ab.COD_BENEFICIO = @cod_beneficio
                        ) 
                        AND c.ESTADO = 1 
                        AND TIPO = 'P' 
                        AND PAGO = 1 
                        order by abv.PRIORIDAD asc";

        public const string registroGCategoria = @"
                    select abv.* 
                    FROM AFI_BENE_VALIDA_CATEGORIA c 
                    left join AFI_BENE_VALIDACIONES abv ON abv.COD_VAL = c.COD_VAL
                    WHERE COD_CATEGORIA = 
                    (
	                    SELECT ab.COD_CATEGORIA 
                        FROM AFI_BENEFICIOS ab 
                        WHERE ab.COD_BENEFICIO = @CodBeneficio
                    ) 
                    AND c.ESTADO = 1 
                    AND TIPO = 'G' 
                    AND REGISTRO = 1 
                    order by abv.PRIORIDAD asc";

        public const string pagoGCategoria = @"
                    select abv.* 
                    FROM AFI_BENE_VALIDA_CATEGORIA c 
                    left join AFI_BENE_VALIDACIONES abv ON abv.COD_VAL = c.COD_VAL
                    WHERE COD_CATEGORIA = 
                    (
	                    SELECT ab.COD_CATEGORIA 
                        FROM AFI_BENEFICIOS ab 
                        WHERE ab.COD_BENEFICIO = @CodBeneficio
                    ) 
                    AND c.ESTADO = 1 
                    AND TIPO = 'G' 
                    AND PAGO = 1 
                    order by abv.PRIORIDAD asc";

        public const string pagoGDif = @"
                    select abv.*, c.pago_justifica 
                    FROM AFI_BENE_VALIDA_CATEGORIA c 
                    left join AFI_BENE_VALIDACIONES abv ON abv.COD_VAL = c.COD_VAL
                    WHERE COD_CATEGORIA = 
                        (
	                        SELECT ab.COD_CATEGORIA 
                            FROM AFI_BENEFICIOS ab 
                            WHERE ab.COD_BENEFICIO = @CodBeneficio
                        ) 
                      AND c.ESTADO = 1 
                      AND PAGO = 1 
                      AND TIPO != 'G' 
                    order by abv.PRIORIDAD asc";

    }

    public class BeneCategoriaValidaListaRequest
    {
        public string? cod_categoria { get; set; } = string.Empty;
        public string? cod_beneficio { get; set; } = string.Empty;
        public string? cedula { get; set; } = string.Empty;
        public string? usuario { get; set; } = string.Empty;
        public string? id_beneficio { get; set; } = string.Empty;
        public string? monto_usuario { get; set; } = string.Empty;
        public string? sepelio_identificacion { get; set; } = string.Empty;
    }
}