create or replace definer = * `%` view buttons_view as
       
WITH opt_agg AS (
    SELECT
        o.lu_button_id,
        JSON_ARRAYAGG(
                JSON_OBJECT(
                        'field_name', CASE WHEN o.field_name LIKE 'cf-%' THEN cfa.name ELSE o.field_name END,
                        'invert', o.invert,
                        'values',
                        (
                            SELECT JSON_ARRAYAGG(
                                           JSON_OBJECT(
                                                   'id', ov.value,
                                                   'value', COALESCE(
                                                           CASE
                                                               WHEN o.field_name = 'tracker_id' THEN t_opt.name
                                                               WHEN o.field_name = 'current_user' THEN r_opt.name
                                                               WHEN o.field_name = 'project_id' THEN p_opt.name
                                                               WHEN o.field_name = 'status_id' THEN s_opt.name
                                                               WHEN o.field_name = 'assigned_to_id' THEN r_opt.name
                                                               WHEN o.field_name LIKE 'cf-%' AND av.value REGEXP '^[0-9]+$' THEN cfe.name
                                                               ELSE ov.value
                                                               END,
                                                           ov.value
                                                            )
                                           )
                                   )
                            FROM lu_button_option_values ov
                                     LEFT JOIN lu_button_action_values av ON o.id = av.lu_button_action_id
                                     LEFT JOIN trackers t_opt ON o.field_name = 'tracker_id' AND ov.value = t_opt.id
                                     LEFT JOIN roles r_opt ON (o.field_name = 'current_user' OR o.field_name = 'assigned_to_id') AND ov.value = r_opt.id
                                     LEFT JOIN projects p_opt ON o.field_name = 'project_id' AND ov.value = p_opt.id
                                     LEFT JOIN issue_statuses s_opt ON o.field_name = 'status_id' AND ov.value = s_opt.id
                                     LEFT JOIN custom_fields cfa ON o.field_name LIKE 'cf-%'
                                AND CAST(SUBSTRING(o.field_name, 4) AS UNSIGNED) = cfa.id
                                     LEFT JOIN custom_field_enumerations cfe ON (av.value REGEXP '^[0-9]+$')
                            WHERE ov.lu_button_option_id = o.id
                        )
                )
        ) AS OPTIONS_JSON
    FROM lu_button_options o
             LEFT JOIN custom_fields cfa ON o.field_name LIKE 'cf-%'
        AND CAST(SUBSTRING(o.field_name, 4) AS UNSIGNED) = cfa.id
    GROUP BY o.lu_button_id
),
     act_inner AS (
         SELECT
             a.lu_button_id,
             a.action,
             CASE WHEN a.field_name LIKE 'cf-%' THEN cfa.name ELSE a.field_name END AS field_name,
             JSON_ARRAYAGG(
                     JSON_OBJECT(
                             'id', av.value,
                             'value',
                             CASE
                                 WHEN a.action = 'set' THEN COALESCE(
                                         CASE
                                             WHEN a.field_name = 'tracker_id' THEN t_act.name
                                             WHEN a.field_name = 'current_user' THEN r_act.name
                                             WHEN a.field_name = 'project_id' THEN p_act.name
                                             WHEN a.field_name = 'status_id' THEN s_act.name
                                             WHEN a.field_name = 'assigned_to_id' THEN r_act.name
                                             WHEN a.field_name = 'priority_id' THEN pr_act.name
                                             WHEN a.field_name LIKE 'cf-%' AND av.value REGEXP '^[0-9]+$' THEN cfe.name
                                             ELSE NULL
                                             END,
                                         av.value
                                                            )
                                 WHEN a.action = 'display' THEN IF(
                                         av.id IS NOT NULL,
                                         COALESCE(
                                                 CASE
                                                     WHEN a.field_name = 'tracker_id' THEN t_act.name
                                                     WHEN a.field_name = 'current_user' THEN r_act.name
                                                     WHEN a.field_name = 'project_id' THEN p_act.name
                                                     WHEN a.field_name = 'status_id' THEN s_act.name
                                                     WHEN a.field_name = 'assigned_to_id' THEN r_act.name
                                                     WHEN a.field_name = 'priority_id' THEN pr_act.name
                                                     WHEN a.field_name LIKE 'cf-%' AND av.value REGEXP '^[0-9]+$' THEN cfe.name
                                                     ELSE NULL
                                                     END,
                                                 av.value
                                         ),
                                         'Отображается без ограничений'
                                                                )
                                 WHEN a.action = 'clear' THEN ''
                                 ELSE NULL
                                 END
                     )
             ) AS values_array
         FROM lu_button_actions a
                  LEFT JOIN lu_button_action_values av ON a.id = av.lu_button_action_id
                  LEFT JOIN trackers t_act ON a.field_name = 'tracker_id' AND av.value = t_act.id
                  LEFT JOIN roles r_act ON (a.field_name = 'current_user' OR a.field_name = 'assigned_to_id') AND av.value = r_act.id
                  LEFT JOIN projects p_act ON a.field_name = 'project_id' AND av.value = p_act.id
                  LEFT JOIN issue_statuses s_act ON a.field_name = 'status_id' AND av.value = s_act.id
                  LEFT JOIN enumerations pr_act ON a.field_name = 'priority_id' AND av.value = pr_act.id
                  LEFT JOIN custom_fields cfa ON a.field_name LIKE 'cf-%'
             AND CAST(SUBSTRING(a.field_name, 4) AS UNSIGNED) = cfa.id
                  LEFT JOIN custom_field_enumerations cfe ON (av.value REGEXP '^[0-9]+$')
             AND cfe.id = CAST(av.value AS UNSIGNED)
         GROUP BY a.lu_button_id, a.action, CASE WHEN a.field_name LIKE 'cf-%' THEN cfa.name ELSE a.field_name END
     ),
     act_agg AS (
         SELECT
             lu_button_id,
             JSON_ARRAYAGG(
                     JSON_OBJECT(
                             'action', action,
                             'field_name', field_name,
                             'values', values_array
                     )
             ) AS ACTIONS_JSON
         FROM act_inner
         GROUP BY lu_button_id
     )

SELECT
    l.id AS BTN_ID,
    bt.name AS BTN_TYPE,
    TRIM(BOTH '- ".' FROM REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(l.name, '\n', ''),
                                                                                  '...', ''),
                                                                          '""', ''),
                                                                  '---', ''),
                                                          '"', ''),
                                                  ' !ruby/hash-with-ivars:ActionController::Parameterselements:  default:', ''),
                                          'ivars:  :@permitted: false', ''),
                                  ' !ruby/hash:ActiveSupport::HashWithIndifferentAccessdefault:', '')) AS BTN_NAME,
    l.description AS BTN_DESCRIPTION,
    act.ACTIONS_JSON,
    oa.OPTIONS_JSON
FROM lu_buttons l
         JOIN lu_button_types bt ON l.lu_button_type_id = bt.id
         LEFT JOIN opt_agg oa ON l.id = oa.lu_button_id
         LEFT JOIN act_agg act ON l.id = act.lu_button_id
WHERE l.locked = 0
GROUP BY l.id, BTN_NAME, l.description;

DESCRIBE projects_view;