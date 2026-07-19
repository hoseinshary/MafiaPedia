<template>
  <div dir="rtl" class="max-w-4xl mx-auto w-full">
    <div class="mb-6">
      <router-link to="/owner" class="text-xs text-muted hover:text-gold-text transition">&larr; بازگشت به داشبورد</router-link>
      <h1 class="text-xl font-bold text-fg mt-2">مدیریت محصولات</h1>
    </div>

    <div class="mb-6 flex gap-2 flex-wrap">
      <button @click="showCreateProduct = true" class="px-4 py-2 bg-gold hover:opacity-80 text-[#0d0d0f] text-sm rounded font-bold transition">محصول جدید</button>
      <button @click="showCategoryModal = true" class="px-4 py-2 border border-gold/40 text-gold-text hover:bg-gold/10 text-sm rounded font-medium transition">مدیریت دسته‌بندی‌ها</button>
    </div>

    <div v-if="loading" class="flex justify-center py-12">
      <div class="w-6 h-6 border-2 border-gold border-t-transparent rounded-full animate-spin" />
    </div>

    <template v-else>
      <div v-if="categories.length > 0" class="mb-4 flex gap-2 flex-wrap">
        <button @click="selectedCategory = null" class="px-3 py-1.5 text-xs rounded transition" :class="selectedCategory === null ? 'bg-gold text-[#0d0d0f] font-bold' : 'bg-surface text-muted hover:text-fg border border-border'">همه</button>
        <button v-for="cat in categories" :key="cat.id" @click="selectedCategory = cat.id" class="px-3 py-1.5 text-xs rounded transition" :class="selectedCategory === cat.id ? 'bg-gold text-[#0d0d0f] font-bold' : 'bg-surface text-muted hover:text-fg border border-border'">{{ cat.name }}</button>
      </div>

      <div v-if="filteredProducts.length === 0" class="text-center py-12 text-muted text-sm">هیچ محصولی یافت نشد</div>

      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
        <div v-for="p in filteredProducts" :key="p.id" class="bg-surface border border-border rounded-xl p-4">
          <div class="flex items-start justify-between">
            <div>
              <p class="text-sm text-fg font-medium">{{ p.name }}</p>
              <p class="text-xs text-muted mt-0.5">{{ p.categoryName }}</p>
            </div>
            <span v-if="p.isActive === false" class="text-xs text-danger">غیرفعال</span>
          </div>
          <p class="text-sm text-gold-text mt-2 font-bold">{{ p.price.toLocaleString() }} تومان</p>
          <div class="flex gap-2 mt-3 pt-3 border-t border-border">
            <button @click="startEditProduct(p)" class="text-xs text-muted hover:text-gold-text transition">ویرایش</button>
            <button @click="handleDeleteProduct(p)" class="text-xs text-danger hover:opacity-80 transition">حذف</button>
          </div>
        </div>
      </div>
    </template>

    <Teleport to="body">
      <div v-if="showCreateProduct || editingProduct" dir="rtl" class="fixed inset-0 z-50 flex items-center justify-center bg-black/60" @click.self="closeProductModal">
        <div class="bg-surface border border-border rounded-xl p-6 w-full max-w-md mx-4">
          <h2 class="text-lg font-bold text-fg mb-4">{{ editingProduct ? 'ویرایش محصول' : 'محصول جدید' }}</h2>
          <div class="space-y-4">
            <div>
              <label class="text-sm text-muted">نام محصول</label>
              <input v-model="productForm.name" type="text" class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg focus:outline-none focus:border-gold transition" />
            </div>
            <div>
              <label class="text-sm text-muted">دسته‌بندی</label>
              <select v-model="productForm.categoryId" class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg focus:outline-none focus:border-gold transition">
                <option value="" disabled>انتخاب دسته</option>
                <option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
              </select>
            </div>
            <div>
              <label class="text-sm text-muted">قیمت (تومان)</label>
              <input v-model.number="productForm.price" type="number" min="0" class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg focus:outline-none focus:border-gold transition ltr" />
            </div>
            <label class="flex items-center gap-2 cursor-pointer">
              <input type="checkbox" v-model="productForm.isActive" class="w-4 h-4 accent-gold" />
              <span class="text-sm text-fg">فعال</span>
            </label>
          </div>
          <p v-if="productError" class="text-sm text-danger mt-3">{{ productError }}</p>
          <div class="flex gap-3 justify-end mt-6 pt-4 border-t border-border">
            <button @click="closeProductModal" class="px-4 py-2 border border-border text-muted hover:text-fg text-sm rounded font-medium transition">انصراف</button>
            <button @click="handleSubmitProduct" :disabled="!productForm.name || !productForm.categoryId || productForm.price <= 0 || submitting" class="px-6 py-2 bg-gold hover:opacity-80 disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] text-sm rounded font-bold transition inline-flex items-center gap-2">
              <div v-if="submitting" class="w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
              {{ editingProduct ? 'ویرایش' : 'ایجاد' }}
            </button>
          </div>
        </div>
      </div>
    </Teleport>

    <Teleport to="body">
      <div v-if="showCategoryModal" dir="rtl" class="fixed inset-0 z-50 flex items-center justify-center bg-black/60" @click.self="showCategoryModal = false">
        <div class="bg-surface border border-border rounded-xl p-6 w-full max-w-md mx-4">
          <h2 class="text-lg font-bold text-fg mb-4">مدیریت دسته‌بندی‌ها</h2>
          <div class="flex gap-2 mb-4">
            <input v-model="newCategoryName" type="text" placeholder="نام دسته جدید" class="flex-1 bg-input border border-border rounded px-4 py-2 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition" @keyup.enter="handleCreateCategory" />
            <button @click="handleCreateCategory" :disabled="!newCategoryName.trim() || catSubmitting" class="px-4 py-2 bg-gold hover:opacity-80 disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] text-sm rounded font-bold transition">افزودن</button>
          </div>
          <div v-if="categories.length === 0" class="text-center py-6 text-muted text-xs">هیچ دسته‌بندی وجود ندارد</div>
          <div v-for="cat in categories" :key="cat.id" class="flex items-center justify-between py-2 border-b border-border last:border-0">
            <span class="text-sm text-fg">{{ cat.name }}</span>
            <div class="flex gap-2">
              <button @click="startEditCategory(cat)" class="text-xs text-muted hover:text-gold-text transition">ویرایش</button>
              <button @click="handleDeleteCategory(cat)" class="text-xs text-danger hover:opacity-80 transition">حذف</button>
            </div>
          </div>
          <div v-if="editCategory" class="flex gap-2 mt-4 pt-4 border-t border-border">
            <input v-model="editCategoryName" type="text" class="flex-1 bg-input border border-border rounded px-4 py-2 text-sm text-fg focus:outline-none focus:border-gold transition" />
            <button @click="handleUpdateCategory" :disabled="!editCategoryName.trim() || catSubmitting" class="px-4 py-2 bg-gold hover:opacity-80 disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] text-sm rounded font-bold transition">ذخیره</button>
            <button @click="editCategory = null" class="px-4 py-2 border border-border text-muted hover:text-fg text-sm rounded transition">انصراف</button>
          </div>
          <button @click="showCategoryModal = false" class="mt-4 w-full px-4 py-2 border border-border text-muted hover:text-fg text-sm rounded font-medium transition">بستن</button>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { FinanceApi } from '@/api'
import { useAuthStore } from '@/stores/authStore'
import type { ProductDto, ProductCategoryDto, CreateProductDto, UpdateProductDto, CreateProductCategoryDto } from '@/types/finance'

const authStore = useAuthStore()
const clubId = () => authStore.activeClubId!

const products = ref<ProductDto[]>([])
const categories = ref<ProductCategoryDto[]>([])
const loading = ref(true)
const selectedCategory = ref<number | null>(null)

const filteredProducts = computed(() => selectedCategory.value ? products.value.filter(p => p.categoryId === selectedCategory.value) : products.value)

const showCreateProduct = ref(false)
const editingProduct = ref<ProductDto | null>(null)
const submitting = ref(false)
const productError = ref('')
const productForm = ref<{ name: string; categoryId: number | ''; price: number; isActive: boolean }>({ name: '', categoryId: '', price: 0, isActive: true })

const showCategoryModal = ref(false)
const newCategoryName = ref('')
const catSubmitting = ref(false)
const editCategory = ref<ProductCategoryDto | null>(null)
const editCategoryName = ref('')

onMounted(loadAll)

async function loadAll() {
  loading.value = true
  try {
    const [pRes, cRes] = await Promise.all([
      FinanceApi.getProducts(clubId()),
      FinanceApi.getProductCategories(clubId()),
    ])
    products.value = pRes.data
    categories.value = cRes.data
  } catch { /* toast handled by interceptor */ }
  finally { loading.value = false }
}

function startEditProduct(p: ProductDto) {
  editingProduct.value = p
  productForm.value = { name: p.name, categoryId: p.categoryId, price: p.price, isActive: p.isActive !== false }
  productError.value = ''
  showCreateProduct.value = false
}

function closeProductModal() {
  showCreateProduct.value = false
  editingProduct.value = null
  productForm.value = { name: '', categoryId: '', price: 0, isActive: true }
  productError.value = ''
}

async function handleSubmitProduct() {
  submitting.value = true
  productError.value = ''
  try {
    if (editingProduct.value) {
      const dto: UpdateProductDto = { name: productForm.value.name, categoryId: productForm.value.categoryId as number, price: productForm.value.price, isActive: productForm.value.isActive }
      await FinanceApi.updateProduct(clubId(), editingProduct.value.id, dto)
    } else {
      const dto: CreateProductDto = { name: productForm.value.name, categoryId: productForm.value.categoryId as number, price: productForm.value.price, isActive: productForm.value.isActive }
      await FinanceApi.createProduct(clubId(), dto)
    }
    closeProductModal()
    products.value = (await FinanceApi.getProducts(clubId())).data
  } catch (err: any) {
    productError.value = err.response?.data?.message || 'خطا در ذخیره محصول'
  } finally {
    submitting.value = false
  }
}

async function handleDeleteProduct(p: ProductDto) {
  if (!confirm(`آیا از حذف محصول "${p.name}" اطمینان دارید؟`)) return
  try {
    await FinanceApi.deleteProduct(clubId(), p.id)
    products.value = (await FinanceApi.getProducts(clubId())).data
  } catch { /* toast handled by interceptor */ }
}

// Category handlers
async function handleCreateCategory() {
  if (!newCategoryName.value.trim()) return
  catSubmitting.value = true
  try {
    const dto: CreateProductCategoryDto = { name: newCategoryName.value.trim() }
    await FinanceApi.createProductCategory(clubId(), dto)
    newCategoryName.value = ''
    categories.value = (await FinanceApi.getProductCategories(clubId())).data
  } catch { /* toast handled by interceptor */ }
  finally { catSubmitting.value = false }
}

function startEditCategory(cat: ProductCategoryDto) {
  editCategory.value = cat
  editCategoryName.value = cat.name
}

async function handleUpdateCategory() {
  if (!editCategory.value || !editCategoryName.value.trim()) return
  catSubmitting.value = true
  try {
    await FinanceApi.updateProductCategory(clubId(), editCategory.value.id, { name: editCategoryName.value.trim() })
    editCategory.value = null
    editCategoryName.value = ''
    categories.value = (await FinanceApi.getProductCategories(clubId())).data
  } catch { /* toast handled by interceptor */ }
  finally { catSubmitting.value = false }
}

async function handleDeleteCategory(cat: ProductCategoryDto) {
  if (!confirm(`آیا از حذف دسته "${cat.name}" اطمینان دارید؟`)) return
  try {
    await FinanceApi.deleteProductCategory(clubId(), cat.id)
    categories.value = (await FinanceApi.getProductCategories(clubId())).data
    products.value = (await FinanceApi.getProducts(clubId())).data
  } catch { /* toast handled by interceptor */ }
}
</script>
