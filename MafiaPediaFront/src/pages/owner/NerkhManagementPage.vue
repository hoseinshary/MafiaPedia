<template>
  <div dir="rtl" class="max-w-4xl mx-auto w-full">
    <div class="mb-6">
      <router-link to="/owner" class="text-xs text-muted hover:text-gold-text transition">&larr; بازگشت به داشبورد</router-link>
      <h1 class="text-xl font-bold text-fg mt-2">مدیریت نرخ‌ها</h1>
    </div>

    <div class="mb-6 flex gap-2">
      <button @click="showCreate = true" class="px-4 py-2 bg-gold hover:opacity-80 text-[#0d0d0f] text-sm rounded font-bold transition">نرخ جدید</button>
    </div>

    <div v-if="loading" class="flex justify-center py-12">
      <div class="w-6 h-6 border-2 border-gold border-t-transparent rounded-full animate-spin" />
    </div>

    <div v-else-if="nerkhs.length === 0" class="text-center py-12 text-muted text-sm">هیچ نرخی ثبت نشده است</div>

    <div v-else class="space-y-3">
      <div v-for="n in nerkhs" :key="n.id" class="bg-surface border border-border rounded-xl p-4 flex items-center justify-between">
        <div>
          <p class="text-sm text-fg font-medium">{{ n.name }}</p>
          <p class="text-xs text-muted mt-1">{{ n.price.toLocaleString() }} تومان</p>
        </div>
        <div class="flex items-center gap-3">
          <span v-if="n.isDefault" class="text-xs bg-gold/20 text-gold-text px-2 py-0.5 rounded">پیش‌فرض</span>
          <span v-if="n.isActive === false" class="text-xs text-danger">غیرفعال</span>
          <button @click="startEdit(n)" class="text-xs text-muted hover:text-gold-text transition">ویرایش</button>
          <button @click="handleDelete(n)" class="text-xs text-danger hover:opacity-80 transition">حذف</button>
        </div>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showCreate || editingItem" dir="rtl" class="fixed inset-0 z-50 flex items-center justify-center bg-black/60" @click.self="closeModal">
        <div class="bg-surface border border-border rounded-xl p-6 w-full max-w-md mx-4">
          <h2 class="text-lg font-bold text-fg mb-4">{{ editingItem ? 'ویرایش نرخ' : 'نرخ جدید' }}</h2>
          <div class="space-y-4">
            <div>
              <label class="text-sm text-muted">نام نرخ</label>
              <input v-model="form.name" type="text" class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg focus:outline-none focus:border-gold transition" />
            </div>
            <div>
              <label class="text-sm text-muted">قیمت (تومان)</label>
              <input v-model.number="form.price" type="number" min="0" class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg focus:outline-none focus:border-gold transition ltr" />
            </div>
            <label class="flex items-center gap-2 cursor-pointer">
              <input type="checkbox" v-model="form.isDefault" class="w-4 h-4 accent-gold" />
              <span class="text-sm text-fg">پیش‌فرض</span>
            </label>
          </div>
          <p v-if="submitError" class="text-sm text-danger mt-3">{{ submitError }}</p>
          <div class="flex gap-3 justify-end mt-6 pt-4 border-t border-border">
            <button @click="closeModal" class="px-4 py-2 border border-border text-muted hover:text-fg text-sm rounded font-medium transition">انصراف</button>
            <button @click="handleSubmit" :disabled="!form.name || form.price <= 0 || submitting" class="px-6 py-2 bg-gold hover:opacity-80 disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] text-sm rounded font-bold transition inline-flex items-center gap-2">
              <div v-if="submitting" class="w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
              {{ editingItem ? 'ویرایش' : 'ایجاد' }}
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { FinanceApi } from '@/api'
import { useAuthStore } from '@/stores/authStore'
import type { NerkhDto, CreateNerkhDto, UpdateNerkhDto } from '@/types/finance'

const authStore = useAuthStore()
const clubId = () => authStore.activeClubId!

const nerkhs = ref<NerkhDto[]>([])
const loading = ref(true)
const showCreate = ref(false)
const editingItem = ref<NerkhDto | null>(null)
const submitting = ref(false)
const submitError = ref('')
const form = ref<{ name: string; price: number; isDefault: boolean }>({ name: '', price: 0, isDefault: false })

onMounted(loadNerkhs)

async function loadNerkhs() {
  loading.value = true
  try {
    const res = await FinanceApi.getNerkhs(clubId())
    nerkhs.value = res.data
  } catch { /* toast handled by interceptor */ }
  finally { loading.value = false }
}

function startEdit(n: NerkhDto) {
  editingItem.value = n
  form.value = { name: n.name, price: n.price, isDefault: n.isDefault }
  submitError.value = ''
}

function closeModal() {
  showCreate.value = false
  editingItem.value = null
  form.value = { name: '', price: 0, isDefault: false }
  submitError.value = ''
}

async function handleSubmit() {
  submitting.value = true
  submitError.value = ''
  try {
    if (editingItem.value) {
      const dto: UpdateNerkhDto = { name: form.value.name, price: form.value.price, isDefault: form.value.isDefault }
      await FinanceApi.updateNerkh(clubId(), editingItem.value.id, dto)
    } else {
      const dto: CreateNerkhDto = { name: form.value.name, price: form.value.price, isDefault: form.value.isDefault }
      await FinanceApi.createNerkh(clubId(), dto)
    }
    closeModal()
    await loadNerkhs()
  } catch (err: any) {
    submitError.value = err.response?.data?.message || 'خطا در ذخیره نرخ'
  } finally {
    submitting.value = false
  }
}

async function handleDelete(n: NerkhDto) {
  if (!confirm(`آیا از حذف نرخ "${n.name}" اطمینان دارید؟`)) return
  try {
    await FinanceApi.deleteNerkh(clubId(), n.id)
    await loadNerkhs()
  } catch { /* toast handled by interceptor */ }
}
</script>
