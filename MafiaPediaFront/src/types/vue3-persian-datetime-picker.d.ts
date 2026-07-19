declare module 'vue3-persian-datetime-picker' {
  import type { DefineComponent } from 'vue'
  const component: DefineComponent<{
    modelValue?: string | number | Date | any[]
    initialValue?: string | number
    format?: string
    inputFormat?: string
    displayFormat?: string
    type?: 'date' | 'datetime' | 'time' | 'year' | 'month'
    view?: 'day' | 'month' | 'year' | 'time'
    color?: string
    label?: string
    placeholder?: string
    clearable?: boolean
    disabled?: boolean
    readonly?: boolean
    editable?: boolean
    simple?: boolean
    min?: string
    max?: string
  }>
  export default component
}
