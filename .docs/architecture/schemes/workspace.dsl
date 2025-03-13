workspace "Name" "Description" {

    !identifiers hierarchical
    !docs docs
    !adrs adr

    model {
        prometheus = element "Prometheus" {
            tags "Prometheus"
        }        
        grafana = element "Grafana" {
            tags "Grafana", "Green"
        }
        jaeger = element "Jaeger" {
            tags "Jaeger"
        }
        ss = softwareSystem "Prototype" {
            host = container "Host" {
                description "Запускает Activities"

                act_main = component "Main:Entry Activity" {
                    tags "Main_Entry_Activity"
                }
                act_lib1_entry = component "Lib1:Entry Activity" {
                    tags "Lib1_Entry_Activity", "Red"
                }
                act_lib1_a = component "Lib1:A Activity" {
                    tags "Lib1_A_Activity", "Red"
                }
                act_lib1_b = component "Lib1:B Activity" {
                    tags "Lib1_B_Activity", "Red"
                }
                act_lib1_c = component "Lib1:C Activity" {
                    tags "Lib1_C_Activity", "Red"
                }
                act_lib1_d = component "Lib1:D Activity" {
                    tags "Lib1_D_Activity", "Red"
                }
                act_lib2_entry = component "Lib2:Entry Activity" {
                    tags "Lib2_Entry_Activity", "Gray"
                }
                act_lib2_a = component "Lib2:A Activity" {
                    tags "Lib2_A_Activity", "Gray"
                }
                act_lib2_b = component "Lib2:B Activity" {
                    tags "Lib2_B_Activity", "Gray"
                }
                activity_scheduler = component "Activity Scheduler" {
                    tags "Activity_Scheduler", "Scheduler", "Green"
                }
                main_activity_scheduler = component "Main:Entry Activity Scheduler" {
                    tags "Main_Activity_Scheduler", "Scheduler", "Green"
                }
                main_activity_executor = component "Main:Entry Activity Executor" {
                    tags "Main_Activity_Executor", "Robot"
                }
                activity_executor = component "Activity Executor" {
                    tags "Activity_Executor", "Robot"
                }
            }

            act_main = container "Main:Entry Activity" {
                tags "Container_Main_Entry_Activity"
            }
            act_lib1_entry = container "Lib1:Entry Activity" {
                tags "Container_Lib1_Entry_Activity"
            }
            act_lib1_a = container "Lib1:A Activity" {
                tags "Container_Lib1_A_Activity"
            }
            act_lib1_b = container "Lib1:B Activity" {
                tags "Container_Lib1_B_Activity"
            }
            act_lib1_c = container "Lib1:C Activity" {
                tags "Container_Lib1_C_Activity"
            }
            act_lib1_d = container "Lib1:D Activity" {
                tags "Container_Lib1_D_Activity"
            }
            act_lib2_entry = container "Lib2:Entry Activity" {
                tags "Container_Lib2_Entry_Activity"
            }
            act_lib2_a = container "Lib2:A Activity" {
                tags "Container_Lib2_A_Activity"
            }
            act_lib2_b = container "Lib2:B Activity " {
                tags "Container_Lib2_B_Activity"
            }

            activity = container "Activity" {
                tags "Activity"

                execute = component "Execute" { 
                    tags "ActivityExecute" 
                }

                prologue = component "Prologue" { 
                    tags "ActivityPrologue", "Gray"
                }

                execute_internal = component "ExecuteInternal" { 
                    tags "ActivityExecuteInternal", "Blue"
                }

                do_work = component "DoWork" { 
                    tags "ActivityDoWork", "Blue"
                    description "Обрабатывает Work Items"
                }

                enqueue_next = component "EnqueueNextActivity" { 
                    tags "ActivityEnqueueNext", "Blue"
                    description "Ставит в очередь следующую activity"
                }

                epilogue = component "Epilogue" { 
                    tags "ActivityEpilogue", "Gray"
                }

                success = component "Success" { 
                    tags "ActivitySuccess", "Green"
                }

                error = component "Error" { 
                    tags "ActivityError", "Red"
                }
            }
        }

        # Component relationships
        ss.host.main_activity_executor -> ss.host.act_main "Запускает"
        ss.host.act_main -> ss.host.activity_scheduler "Lib1:Entry Descriptor"
        ss.host.act_main -> ss.host.activity_scheduler "Lib2:Entry Descriptor"
        ss.host.main_activity_scheduler -> ss.host.main_activity_executor "Отправляет Main:Entry на исполнение по графику"
        ss.host.activity_scheduler -> ss.host.activity_executor "Отправляет Activity на исполнение по графику"
        ss.host.activity_executor -> ss.host.act_lib1_entry "Запускает"
        ss.host.activity_executor -> ss.host.act_lib1_a "Запускает"
        ss.host.activity_executor -> ss.host.act_lib1_b "Запускает"
        ss.host.activity_executor -> ss.host.act_lib1_c "Запускает"
        ss.host.activity_executor -> ss.host.act_lib1_d "Запускает"
        ss.host.act_lib1_entry -> ss.host.activity_scheduler "Ставит в очередь Lib1:A"
        ss.host.act_lib1_a -> ss.host.activity_scheduler "Ставит в очередь Lib1:B"
        ss.host.act_lib1_b -> ss.host.activity_scheduler "Ставит в очередь Lib1:C"
        ss.host.act_lib1_c -> ss.host.activity_scheduler "Ставит в очередь Lib1:D"
        ss.host.activity_executor -> ss.host.act_lib2_entry "Запускает"
        ss.host.activity_executor -> ss.host.act_lib2_a "Запускает"
        ss.host.activity_executor -> ss.host.act_lib2_b "Запускает"
        ss.host.act_lib2_entry -> ss.host.activity_scheduler "Ставит в очередь Lib2:A"
        ss.host.act_lib2_a -> ss.host.activity_scheduler "Ставит в очередь Lib2:B"

        ss.activity.execute -> ss.activity.prologue
        ss.activity.prologue -> ss.activity.execute_internal
        ss.activity.execute_internal -> ss.activity.do_work
        ss.activity.execute_internal -> ss.activity.enqueue_next
        ss.activity.execute_internal -> ss.activity.epilogue
        ss.activity.epilogue -> ss.activity.success
        ss.activity.epilogue -> ss.activity.error
        rel_act_exec_jaegers = ss.activity.execute -> jaeger "Создает tracing span"
        prol_to_prom = ss.activity.prologue -> prometheus "Инкрементирует счетчик выполняющихся activities"
        ss.activity.error -> prometheus "Инкрементирует счетчик ошибок"
        ss.activity.epilogue -> prometheus "Уменьшает счетчик выполняющихся activities"
        ss.activity.epilogue -> prometheus "Сохраняет время выполнения activity"
        ss.activity.error -> jaeger "Завершает tracing span"
        ss.activity.success -> jaeger "Завершает tracing span"

        ss.host -> ss.act_main "Запускает по графику"

        ss.act_main -> ss.act_lib1_entry "Запускает (ставит в очередь)"
        ss.act_lib1_entry -> ss.act_lib1_a "Запускает (ставит в очередь)"
        ss.act_lib1_a -> ss.act_lib1_b "Запускает (ставит в очередь)"
        ss.act_lib1_b -> ss.act_lib1_c "Запускает (ставит в очередь)"
        ss.act_lib1_c -> ss.act_lib1_d "Запускает (ставит в очередь)"
        
        ss.act_main -> ss.act_lib2_entry "Запускает (ставит в очередь)"
        ss.act_lib2_entry -> ss.act_lib2_a "Запускает (ставит в очередь)"
        ss.act_lib2_a -> ss.act_lib2_b "Запускает (ставит в очередь)"

        ss.act_main -> prometheus "Отправляет метрики"
        ss.act_lib1_entry -> prometheus "Отправляет метрики"
        ss.act_lib1_a -> prometheus "Отправляет метрики"
        ss.act_lib1_b -> prometheus "Отправляет метрики"
        ss.act_lib1_c -> prometheus "Отправляет метрики"
        ss.act_lib2_entry -> prometheus "Отправляет метрики"
        ss.act_lib2_a -> prometheus "Отправляет метрики"
        ss.act_lib2_b -> prometheus "Отправляет метрики"

        ss.act_main -> jaeger "Отправляет трейсы"
        ss.act_lib1_entry -> jaeger "Отправляет трейсы"
        ss.act_lib1_a -> jaeger "Отправляет трейсы"
        ss.act_lib1_b -> jaeger "Отправляет трейсы"
        ss.act_lib1_c -> jaeger "Отправляет трейсы"
        ss.act_lib2_entry -> jaeger "Отправляет трейсы"
        ss.act_lib2_a -> jaeger "Отправляет трейсы"
        ss.act_lib2_b -> jaeger "Отправляет трейсы"

        ss -> prometheus "Отправляет метрики" {
            tags "Send_Metrics_Rel"
        }
        ss -> jaeger "Отправляет трейсы"
        grafana -> prometheus "Отображает метрики"
        grafana -> jaeger "Отображает трейсы"
        
        #ss.host.act_main -> ss.host.act_lib1_entry "Ставит в очередь"
    }

    views {
        systemContext ss "SystemContext" {
            include *            
            include prometheus
            include grafana
            include jaeger
            exclude prol_to_prom rel_act_exec_jaegers
            include "grafana -> prometheus"                        
        }

        container ss "ActivityFlow" {
            include *
            #exclude prometheus
            #exclude jaeger
            exclude ss.activity
            autolayout lr
        }

        component ss.host "01_Host" {
            include *
            autolayout lr
        }

        component ss.activity "02_ActivityFlow" {
            include *
            autolayout lr
        }

        styles {
            relationship "Send_Metrics_Rel" {
                position 50
            }
            element "Element" {
                color #ffffff
                shape RoundedBox
            }
            element "Person" {
                background #d34407
                shape person
            }
            element "Software System" {
                background #f86628
                shape RoundedBox
            }
            element "Container" {
                background #f88728
                shape RoundedBox
            }
            element "Component" {
                background #f88728
                shape RoundedBox
            }
            element "Green" {
                background #2b6e1e
            }
            element "Red" {
                background #ca000f
            }
            element "Blue" {
                background #4249af
                shape RoundedBox
            }
            element "Gray" {
                background #808189
                shape RoundedBox
            }
            element "Robot" {
                background #474E93
                shape Robot
            }
            element "Jaeger" {
                width 250                
                color #ffffff
                background #474E93
                fontSize 12
                shape Cylinder
            }
            element "Prometheus" {
                width 250                
                color #ffffff
                background #bf3f23
                fontSize 12
                shape Cylinder
            }
            element "Scheduler" {
                color #ffffff
                fontSize 12
                shape Cylinder
            }
        }
    }

    configuration {
        scope softwaresystem
    }
}