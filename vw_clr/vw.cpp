#include "vw_clr.h"

namespace Microsoft
{
	namespace Research
	{
		namespace MachineLearning
		{
			VowpalWabbit::VowpalWabbit(System::String^ pArgs)
				: VowpalWabbitBase(pArgs)
			{
			}

			vw* wrapped_seed_vw_model(vw* vw)
			{
				try
				{
					return VW::seed_vw_model(vw, "");
				}
				catch (std::exception const& ex)
				{
					throw gcnew System::Exception(gcnew System::String(ex.what()));
				}
			}

			VowpalWabbit::VowpalWabbit(VowpalWabbitModel^ model)
				: VowpalWabbitBase(wrapped_seed_vw_model(model->m_vw)), m_model(model)
			{
				m_model->IncrementReference();
			}

			VowpalWabbit::~VowpalWabbit()
			{
				this->!VowpalWabbit();
			}

			VowpalWabbit::!VowpalWabbit()
			{
				if (m_model != nullptr)
				{
					// this object doesn't own the VW instance
					m_model->DecrementReference();
					m_model = nullptr;
				}
				else
				{
					// this object owns the VW instance.
					this->InternalDispose();
				}
			}

			uint32_t VowpalWabbit::HashSpace(System::String^ s)
			{
				auto string = msclr::interop::marshal_as<std::string>(s);

				try
				{
					return VW::hash_space(*m_vw, string);
				}
				catch (std::exception const& ex)
				{
					throw gcnew System::Exception(gcnew System::String(ex.what()));
				}
			}

			uint32_t VowpalWabbit::HashFeature(System::String^ s, unsigned long u)
			{
				try
				{
					auto string = msclr::interop::marshal_as<std::string>(s);
					return VW::hash_feature(*m_vw, string, u);
				}
				catch (std::exception const& ex)
				{
					throw gcnew System::Exception(gcnew System::String(ex.what()));
				}
			}

			generic<typename TPrediction>
				where TPrediction : VowpalWabbitPrediction, gcnew(), ref class
			TPrediction VowpalWabbit::PredictOrLearn(System::String^ line, bool predict)
			{
				auto bytes = System::Text::Encoding::UTF8->GetBytes(line);
				auto lineHandle = GCHandle::Alloc(bytes, GCHandleType::Pinned);

				example* ex = nullptr;
				try
				{
					ex = VW::read_example(*m_vw, reinterpret_cast<char*>(lineHandle.AddrOfPinnedObject().ToPointer()));

					if (predict)
						m_vw->l->predict(*ex);
					else
						m_vw->learn(ex);

					auto prediction = gcnew TPrediction();
					prediction->ReadFromExample(m_vw, ex);

					m_vw->l->finish_example(*m_vw, *ex);
					ex = nullptr;

					return prediction;
				}
				catch (std::exception const& ex)
				{
					throw gcnew System::Exception(gcnew System::String(ex.what()));
				}
				finally
				{
					lineHandle.Free();

					if (ex != nullptr)
					{
						VW::finish_example(*m_vw, ex);
					}
				}
			}

			generic<typename TPrediction>
				where TPrediction : VowpalWabbitPrediction, gcnew(), ref class
			TPrediction VowpalWabbit::Learn(System::String^ line)
			{
				return PredictOrLearn<TPrediction>(line, false);
			}

			generic<typename TPrediction>
				where TPrediction : VowpalWabbitPrediction, gcnew(), ref class
			TPrediction VowpalWabbit::Predict(System::String^ line)
			{
				return PredictOrLearn<TPrediction>(line, true);
			}

			void VowpalWabbit::Driver()
			{
				LEARNER::generic_driver(*m_vw);
			}
		}
	}
}