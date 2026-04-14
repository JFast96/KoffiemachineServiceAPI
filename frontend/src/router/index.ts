import { createRouter, createWebHistory } from 'vue-router'
import MachineList from '@/components/MachineList.vue'
import MachineDetail from '@/components/MachineDetail.vue'
import MachineForm from '@/components/MachineForm.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'machines',
      component: MachineList
    },
    {
      path: '/machines/new',
      name: 'machine-create',
      component: MachineForm
    },
    {
      path: '/machines/:id',
      name: 'machine-detail',
      component: MachineDetail,
      props: true
    }
  ]
})

export default router
