<template>
  <div dir="rtl" class="min-h-screen flex items-center justify-center bg-transparent relative overflow-hidden" style="font-family:'Vazirmatn',sans-serif;">
    <span class="absolute select-none pointer-events-none" style="font-size:80px;font-weight:700;color:#c9b07a;opacity:0.15;top:50%;left:50%;transform:translate(-50%,-50%);">
      404
    </span>
    <div class="relative z-10" style="background:#141416;border:0.5px solid rgba(255,255,255,0.07);border-radius:14px;padding:2.5rem;text-align:center;max-width:400px;width:90%;">
      <div style="font-size:40px;margin-bottom:1rem;">🃏</div>
      <div style="color:#e8e4d9;font-size:22px;font-weight:700;">صفحه‌ای پیدا نشد</div>
      <div style="color:rgba(232,228,217,0.4);font-size:14px;margin-top:0.5rem;">این صفحه وجود ندارد یا حذف شده است</div>
      <div class="flex justify-center gap-3" style="margin-top:1.5rem;">
        <button
          class="cursor-pointer"
          style="padding:10px 24px;border-radius:8px;font-size:13px;font-weight:700;background:#c9b07a;color:#0d0d0f;border:none;"
          @click="$router.push('/')"
        >
          بازگشت به خانه
        </button>
        <button
          class="cursor-pointer"
          style="padding:10px 24px;border-radius:8px;font-size:13px;background:transparent;color:rgba(232,228,217,0.6);border:0.5px solid rgba(255,255,255,0.15);"
          @click="$router.push('/ranking/overall')"
        >
          رتبه‌بندی
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
</script>
